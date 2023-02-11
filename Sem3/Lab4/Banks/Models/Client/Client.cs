using System.Text.RegularExpressions;
using Banks.Exceptions;

namespace Banks.Models;

public class Client
{
    public const string PhoneNumberRegex = @"^([\\+]?33[-]?|[0])?[1-9][0-9]{8}$";

    public Client(
        string name,
        string phoneNumber,
        string? passportNumber = null,
        Address? address = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BanksInvalidValueException("Can't infer name from an empty string");
        ValidatePhoneNumber(phoneNumber);
        if (passportNumber != null)
        {
            ValidatePassportNumber(passportNumber!);
            PassportNumber = passportNumber;
        }

        (PhoneNumber, Name, Address) = (phoneNumber, name, address);
    }

    public string Name { get; }
    public string PhoneNumber { get; private set; }
    public string PassportNumber { get; private set; } = string.Empty;
    public Address? Address { get; private set; }

    public bool IsSuspicious => string.IsNullOrWhiteSpace(PassportNumber) && Address is null;

    public static ClientBuilder MakeBuilder() => new ClientBuilder();

    public static void ValidatePhoneNumber(string number)
    {
        ArgumentNullException.ThrowIfNull(number);
        if (Regex.IsMatch(number, PhoneNumberRegex))
            throw new BanksInvalidValueException($"Phone number \"{number}\" doesn't match expected format");
    }

    public static void ValidatePassportNumber(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new BanksInvalidValueException("Can't infer passport number from empty string");
        if (!int.TryParse(number, out int _))
            throw new BanksInvalidValueException($"Passport number \"{number}\" does not match expected format");
    }

    public void SetPhoneNumber(string number)
    {
        ValidatePhoneNumber(number);
        PhoneNumber = number;
    }

    public void SetPassportNumber(string number)
    {
        ValidatePassportNumber(number);
        PassportNumber = number;
    }

    public void SetAddress(Address address)
    {
        ArgumentNullException.ThrowIfNull(address);
        Address = address;
    }

    public void RemovePassport() => PassportNumber = string.Empty;

    public void RemoveAddress() => Address = null;

    internal Client Copy() => (MemberwiseClone() as Client) !;
}