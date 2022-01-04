using System.Collections.Generic;
using Domain.Common;

namespace Domain.ValueObjects
{
    public class Address : ValueObject
    {
        public Address()
        {
        }

        public Address(
            string address1,
            string address2,
            string street,
            string city,
            string state,
            string country,
            string zipcode)
        {
            Address1 = address1 ?? string.Empty;
            Address2 = address2 ?? string.Empty;
            Street = street?? string.Empty;
            City = city?? string.Empty;
            State = state?? string.Empty;
            Country = country?? string.Empty;
            ZipCode = zipcode?? string.Empty;
            UpdateFullAddress();
        }

        public string Address1 { get; private set; }
        public string Address2 { get; private set; }
        public string Street { get; private set; }
        public string City { get; private set; }
        public string State { get; private set; }
        public string Country { get; private set; }
        public string ZipCode { get; private set; }
        public string FullAddress { get; private set; }

        private void UpdateFullAddress()
        {
            FullAddress = Address1 +
                          Address2 +
                          Street +
                          City +
                          State +
                          Country +
                          ZipCode;

        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            // Using a yield return statement to return each element one at a time
            yield return Address1;
            yield return Address2;
            yield return Street;
            yield return City;
            yield return State;
            yield return Country;
            yield return ZipCode;
        }
    }
}