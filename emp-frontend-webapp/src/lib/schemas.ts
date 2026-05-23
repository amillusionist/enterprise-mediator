import { z } from 'zod';
import { ACCEPTED_FILE_TYPES, FILE_UPLOAD_MAX_SIZE } from './constants';

/**
 * Authentication Schemas
 */
export const loginSchema = z.object({
  email: z.string().email('Please enter a valid email address'),
  password: z.string().min(8, 'Password must be at least 8 characters'),
});

export const LoginSchema = loginSchema;

export const registerSchema = z.object({
  inviteToken: z.string().min(1, 'Invite token is required'),
  name: z.string().min(2, 'Name must be at least 2 characters'),
  password: z
    .string()
    .min(8, 'Password must be at least 8 characters')
    .max(128, 'Password must be at most 128 characters')
    .regex(/[A-Z]/, 'Password must contain at least one uppercase letter')
    .regex(/[a-z]/, 'Password must contain at least one lowercase letter')
    .regex(/[0-9]/, 'Password must contain at least one number')
    .regex(/[^A-Za-z0-9]/, 'Password must contain at least one special character'),
});

export const RegisterSchema = registerSchema;

export const registerFormSchema = registerSchema.extend({
  confirmPassword: z.string().min(1, 'Please confirm your password'),
}).refine((data) => data.password === data.confirmPassword, {
  message: 'Passwords do not match',
  path: ['confirmPassword'],
});

export const RegisterFormSchema = registerFormSchema;


export const mfaVerifySchema = z.object({
  code: z
    .string()
    .length(6, 'Code must be exactly 6 digits')
    .regex(/^\d+$/, 'Code must contain only numbers'),
});

export const passwordResetRequestSchema = z.object({
  email: z.string().email('Please enter a valid email address'),
});

export const passwordResetConfirmSchema = z.object({
  token: z.string().min(1, 'Reset token is required'),
  newPassword: z
    .string()
    .min(8, 'Password must be at least 8 characters')
    .max(128, 'Password must be at most 128 characters'),
});

/**
 * Project Schemas
 */
export const createProjectSchema = z.object({
  name: z.string().min(3, 'Project name must be at least 3 characters'),
  clientId: z.string().uuid('Please select a valid client'),
  description: z.string().optional(),
});

/**
 * SOW Upload Schemas
 */
export const sowUploadSchema = z.object({
  file: z
    .custom<File>((val) => val instanceof File, 'Please upload a file')
    .refine(
      (file) => file.size <= FILE_UPLOAD_MAX_SIZE,
      `File size must be less than ${FILE_UPLOAD_MAX_SIZE / 1024 / 1024}MB`
    )
    .refine(
      (file) => ACCEPTED_FILE_TYPES.includes(file.type),
      'Only .pdf, .docx, and .doc formats are supported'
    ),
});

export const SowValidationSchema = z.object({
  size: z.number().max(FILE_UPLOAD_MAX_SIZE, `File too large. Maximum ${FILE_UPLOAD_MAX_SIZE / 1024 / 1024}MB`),
  type: z.string().refine(
    (type) => ACCEPTED_FILE_TYPES.includes(type),
    'Only .pdf, .docx, and .doc formats are supported'
  ),
  name: z.string().min(1, 'File name is required'),
});

/**
 * Project Brief Schema (for HITL review form)
 */
export const projectBriefSchema = z.object({
  title: z.string().min(3, 'Title must be at least 3 characters'),
  summary: z.string().min(10, 'Summary must be at least 10 characters'),
  deliverables: z.array(z.string().min(1)).min(1, 'At least one deliverable is required'),
  requiredSkills: z.array(z.string().min(1)).min(1, 'At least one skill is required'),
  technologies: z.array(z.string()).optional(),
  scope: z.string().optional(),
  estimatedDurationWeeks: z.coerce.number().int().positive().optional(),
  estimatedBudget: z.coerce.number().positive().optional(),
});

/**
 * Vendor Schemas
 */
export const vendorProfileSchema = z.object({
  companyName: z.string().min(2, 'Company name is required'),
  address: z.string().min(5, 'Address is required'),
  primaryContactName: z.string().min(2, 'Contact name is required'),
  primaryContactEmail: z.string().email('Invalid email address'),
  primaryContactPhone: z.string().min(10, 'Valid phone number is required'),
  skills: z.array(z.string()).min(1, 'At least one skill is required'),
  paymentDetails: z.object({
    bankName: z.string().min(2, 'Bank name is required'),
    accountNumber: z.string().min(8, 'Account number is required'),
    swiftCode: z.string().min(8, 'SWIFT code is required'),
  }).optional(),
});

export const VendorSchema = vendorProfileSchema;

/**
 * Client Schemas
 */
export const clientSchema = z.object({
  companyName: z.string().min(2, 'Company name is required'),
  address: z.string().min(5, 'Address is required'),
  contacts: z.array(
    z.object({
      name: z.string().min(2, 'Contact name is required'),
      email: z.string().email('Invalid email address'),
      phone: z.string().optional(),
    })
  ).min(1, 'At least one contact is required'),
});

/**
 * Proposal Schemas
 */
export const proposalSubmissionSchema = z.object({
  cost: z.coerce.number().positive('Cost must be a positive number'),
  timeline: z.string().min(5, 'Timeline description is required'),
  keyPersonnel: z.string().min(10, 'Please list key personnel'),
  file: z.optional(
    z.custom<File>((val) => val instanceof File, 'Invalid file')
      .refine(
        (file) => file.size <= FILE_UPLOAD_MAX_SIZE,
        `File size limit is ${FILE_UPLOAD_MAX_SIZE / 1024 / 1024}MB`
      )
  ),
});

/**
 * Invoice Schemas
 */
export const invoiceSchema = z.object({
  amount: z.coerce.number().positive('Amount must be greater than zero'),
  currency: z.string().length(3, 'Currency must be a 3-letter code'),
  description: z.string().optional(),
  dueDate: z.string().optional(),
});

export const InvoiceSchema = invoiceSchema;

/**
 * User Invite Schema
 */
export const userInviteSchema = z.object({
  email: z.string().email('Invalid email address'),
  role: z.enum(['SystemAdministrator', 'FinanceManager', 'ClientContact', 'VendorContact']),
  clientId: z.string().uuid().optional(),
  vendorId: z.string().uuid().optional(),
});

/**
 * Retention Policy Schema
 */
export const retentionPolicySchema = z.object({
  auditLogRetentionDays: z.coerce.number().min(30).max(3650),
  financialRecordRetentionDays: z.coerce.number().min(365).max(3650),
  projectDataRetentionDays: z.coerce.number().min(90).max(3650),
});

/**
 * User Edit Schema
 */
export const userEditSchema = z.object({
  role: z.enum(['SystemAdministrator', 'FinanceManager', 'ClientContact', 'VendorContact']),
  isActive: z.boolean(),
});

// Inferred types
export type LoginInput = z.infer<typeof loginSchema>;
export type LoginFormData = z.infer<typeof loginSchema>;
export type RegisterFormData = z.infer<typeof registerSchema>;
export type RegisterFormInput = z.infer<typeof registerFormSchema>;
export type MfaVerifyFormData = z.infer<typeof mfaVerifySchema>;
export type CreateProjectFormData = z.infer<typeof createProjectSchema>;
export type SowUploadFormData = z.infer<typeof sowUploadSchema>;
export type VendorProfileFormData = z.infer<typeof vendorProfileSchema>;
export type ClientFormData = z.infer<typeof clientSchema>;
export type ProposalSubmissionFormData = z.infer<typeof proposalSubmissionSchema>;
export type InvoiceFormData = z.infer<typeof invoiceSchema>;
export type UserInviteFormData = z.infer<typeof userInviteSchema>;
export type RetentionPolicyFormData = z.infer<typeof retentionPolicySchema>;
export type UserEditFormData = z.infer<typeof userEditSchema>;
