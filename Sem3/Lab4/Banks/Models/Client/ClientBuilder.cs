using System;
using Banks.Exceptions;

namespace Banks.Models;

public class ClientBuilder
{
    private string? Name { get; set; }
    private string? PhoneNumber { get; set; }
    private string? PassportNumber { get; set; }
    private Address? Address { get; set; }

    public ClientBuilder WithPassportNumber(string number)
    {
        Client.ValidatePassportNumber(number);
        PassportNumber = number;
        return this;
    }

    public ClientBuilder WithAddress(Address address)
    {
        ArgumentNullException.ThrowIfNull(address);
        Address = address;
        return this;
    }

    public ClientBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BanksInvalidValueException("Can't infer name from empty string");
        Name = name;
        return this;
    }

    public ClientBuilder WithPhone(string phone)
    {
        Client.ValidatePhoneNumber(phone);
        PhoneNumber = phone;
        return this;
    }

    public Client Make() => new Client(Name!, PhoneNumber!, PassportNumber, Address);
}