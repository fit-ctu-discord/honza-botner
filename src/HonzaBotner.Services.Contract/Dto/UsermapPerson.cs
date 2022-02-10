using System.Collections.Generic;

namespace HonzaBotner.Services.Contract.Dto;

public class UsermapPerson
{
#nullable disable
    public string Username { get; set; }

    public int PersonalNumber { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string FullName { get; set; }

    public List<string> Emails { get; set; }

    public string PreferredEmail { get; set; }

    public List<UsermapDepartment> Departments { get; set; }

    public List<string> Rooms { get; set; }

    public List<string> Phones { get; set; }

    public List<string> Roles { get; set; }
}
