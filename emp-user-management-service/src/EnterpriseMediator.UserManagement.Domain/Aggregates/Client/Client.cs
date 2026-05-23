using System;
using System.Collections.Generic;
using EnterpriseMediator.UserManagement.Domain.Events;
using EnterpriseMediator.UserManagement.Domain.ValueObjects;
using MediatR;

namespace EnterpriseMediator.UserManagement.Domain.Aggregates.Client
{
    /// <summary>
    /// Aggregate Root representing a Client organization.
    /// </summary>
    public class Client
    {
        public Guid Id { get; private set; }
        public string CompanyName { get; private set; } = null!;
        public Address CompanyAddress { get; private set; } = null!;
        public Address BillingAddress { get; private set; } = null!;
        public string PrimaryContactEmail { get; private set; } = null!;
        public string Status { get; private set; } = null!;
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }

        private readonly List<INotification> _domainEvents = new();
        public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

        // EF Core Constructor
        protected Client() { }

        private Client(string companyName, Address companyAddress, Address billingAddress, string primaryContactEmail)
        {
            Id = Guid.NewGuid();
            CompanyName = companyName;
            CompanyAddress = companyAddress;
            BillingAddress = billingAddress;
            PrimaryContactEmail = primaryContactEmail;
            Status = "Active";
            CreatedAt = DateTimeOffset.UtcNow;
        }

        public static Client Create(string companyName, Address companyAddress, Address billingAddress, string primaryContactEmail)
        {
            if (string.IsNullOrWhiteSpace(companyName)) throw new ArgumentException("Company name is required", nameof(companyName));
            ArgumentNullException.ThrowIfNull(companyAddress);
            ArgumentNullException.ThrowIfNull(billingAddress);
            if (string.IsNullOrWhiteSpace(primaryContactEmail)) throw new ArgumentException("Primary contact email is required", nameof(primaryContactEmail));

            var client = new Client(companyName.Trim(), companyAddress, billingAddress, primaryContactEmail.ToLowerInvariant().Trim());
            client.AddDomainEvent(new ClientCreatedEvent(client.Id, client.CompanyName, client.PrimaryContactEmail, client.CreatedAt));
            return client;
        }

        public void UpdateDetails(string companyName, string primaryContactEmail)
        {
            if (string.IsNullOrWhiteSpace(companyName)) throw new ArgumentException("Company name is required", nameof(companyName));
            if (string.IsNullOrWhiteSpace(primaryContactEmail)) throw new ArgumentException("Primary contact email is required", nameof(primaryContactEmail));

            CompanyName = companyName.Trim();
            PrimaryContactEmail = primaryContactEmail.ToLowerInvariant().Trim();
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void UpdateAddresses(Address companyAddress, Address billingAddress)
        {
            CompanyAddress = companyAddress ?? throw new ArgumentNullException(nameof(companyAddress));
            BillingAddress = billingAddress ?? throw new ArgumentNullException(nameof(billingAddress));
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void Deactivate()
        {
            if (Status != "Inactive")
            {
                Status = "Inactive";
                UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        public void Activate()
        {
            if (Status != "Active")
            {
                Status = "Active";
                UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        private void AddDomainEvent(INotification eventItem)
        {
            _domainEvents.Add(eventItem);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}