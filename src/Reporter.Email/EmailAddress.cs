namespace Reporter.Email
{
    public class EmailAddress
    {
        public EmailAddress(string name, string emailAddress)
        {
            Name = name;
            Address = emailAddress;
        }

        public string Name { get; }

        public string Address { get; }
    }
}