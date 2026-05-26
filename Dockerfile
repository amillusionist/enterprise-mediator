# syntax=docker/dockerfile:1.4
# Primary production app: Next.js frontend (emp-frontend-webapp) — port 3000

ARG PRIMARY_APP=emp-frontend-webapp
ARG APP_PORT=3000
ARG NEXT_PUBLIC_API_URL=http://localhost:5000/api/v1

FROM node:20-alpine AS base
RUN apk add --no-cache libc6-compat
ENV NEXT_TELEMETRY_DISABLED=1

FROM base AS prepare
ARG PRIMARY_APP
WORKDIR /repo
COPY . .
RUN set -eu; \
  resolve_app() { \
    if [ -f "${1}/package.json" ]; then echo "${1}"; return 0; fi; \
    for pkg in $(find . -maxdepth 6 -name package.json -not -path '*/node_modules/*' 2>/dev/null); do \
      if grep -q '"next"' "$pkg" 2>/dev/null; then dirname "$pkg" | sed 's|^\./||'; return 0; fi; \
    done; \
    for pkg in $(find . -maxdepth 6 -name package.json -not -path '*/node_modules/*' 2>/dev/null); do \
      dirname "$pkg" | sed 's|^\./||'; return 0; \
    done; \
    return 1; \
  }; \
  APP_DIR="$(resolve_app "${PRIMARY_APP}")" || { echo "No deployable Node app found" >&2; exit 1; }; \
  echo "${APP_DIR}" > /tmp/APP_DIR; \
  echo "Primary app: ${APP_DIR}" >&2

FROM base AS deps
WORKDIR /app
COPY --from=prepare /repo /repo
COPY --from=prepare /tmp/APP_DIR /tmp/APP_DIR
RUN set -eu; \
  APP_DIR="$(cat /tmp/APP_DIR)"; \
  cd "/repo/${APP_DIR}"; \
  install_root() { \
    cd /repo; \
    if [ -f pnpm-workspace.yaml ] || { [ -f package.json ] && grep -q '"workspaces"' package.json 2>/dev/null; }; then \
      corepack enable pnpm; \
      if [ -f pnpm-lock.yaml ]; then pnpm install --frozen-lockfile; else pnpm install; fi; \
    elif [ -f pnpm-lock.yaml ]; then \
      corepack enable pnpm; pnpm install --frozen-lockfile; \
    elif [ -f yarn.lock ]; then \
      corepack enable yarn; yarn install --immutable; \
    elif [ -f package-lock.json ]; then \
      npm ci --ignore-scripts; \
    else \
      npm install --ignore-scripts; \
    fi; \
  }; \
  install_app() { \
    if [ -f pnpm-lock.yaml ]; then \
      corepack enable pnpm; pnpm install --frozen-lockfile; \
    elif [ -f yarn.lock ]; then \
      corepack enable yarn; yarn install --immutable; \
    elif [ -f package-lock.json ]; then \
      npm ci --ignore-scripts; \
    else \
      npm install --ignore-scripts; \
    fi; \
  }; \
  if [ -f /repo/pnpm-workspace.yaml ] || { [ -f /repo/package.json ] && grep -q '"workspaces"' /repo/package.json 2>/dev/null; }; then \
    install_root; \
  elif [ -f /repo/turbo.json ] && [ -f /repo/package.json ]; then \
    install_root; \
  elif [ -f /repo/nx.json ] && [ -f /repo/package.json ]; then \
    install_root; \
  else \
    install_app; \
  fi; \
  mkdir -p /app; \
  cp -a "/repo/${APP_DIR}"/. /app/; \
  if [ -d /repo/node_modules ] && [ ! -d /app/node_modules ]; then \
    cp -a /repo/node_modules /app/node_modules; \
  fi

FROM base AS builder
WORKDIR /app
ARG PRIMARY_APP
ARG NEXT_PUBLIC_API_URL
ENV NODE_ENV=production
ENV NEXT_PUBLIC_API_URL=${NEXT_PUBLIC_API_URL}
COPY --from=deps /app ./
COPY --from=prepare /repo /repo
COPY --from=prepare /tmp/APP_DIR /tmp/APP_DIR
RUN set -eu; \
  APP_DIR="$(cat /tmp/APP_DIR)"; \
  if [ -f /repo/turbo.json ] && [ -f /repo/package.json ]; then \
    cd /repo; \
    if [ -f pnpm-lock.yaml ]; then corepack enable pnpm && pnpm exec turbo run build --filter="./${APP_DIR}"; \
    elif [ -f yarn.lock ]; then yarn turbo run build --filter="./${APP_DIR}"; \
    else npx turbo run build --filter="./${APP_DIR}"; fi; \
    cp -a "/repo/${APP_DIR}"/. /app/; \
  elif [ -f /repo/nx.json ] && [ -f /repo/package.json ]; then \
    cd /repo; \
    if [ -f pnpm-lock.yaml ]; then corepack enable pnpm && pnpm exec nx build "${APP_DIR}"; \
    else npx nx build "${APP_DIR}"; fi; \
    cp -a "/repo/${APP_DIR}"/. /app/; \
  else \
    npm run build; \
  fi

FROM base AS runner
WORKDIR /app
ARG APP_PORT=3000
ENV NODE_ENV=production
ENV NEXT_TELEMETRY_DISABLED=1
ENV PORT=${APP_PORT}
ENV HOSTNAME=0.0.0.0

RUN addgroup --system --gid 1001 nodejs \
  && adduser --system --uid 1001 --ingroup nodejs nextjs

COPY --from=builder /app/public ./public
RUN mkdir -p .next && chown nextjs:nodejs .next
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static

USER nextjs
EXPOSE 3000

HEALTHCHECK --interval=30s --timeout=5s --start-period=40s --retries=3 \
  CMD node -e "require('http').get('http://127.0.0.1:'+(process.env.PORT||3000),(r)=>{r.resume();process.exit(r.statusCode<500?0:1)}).on('error',()=>process.exit(1))"

CMD ["node", "server.js"]
