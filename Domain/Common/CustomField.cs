namespace Domain.Common
{
    public class CustomField
    {
        public CustomField()
        {
            
        }

        public CustomField(
            string name,
            string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }
}