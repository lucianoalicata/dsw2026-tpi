namespace Dsw2026Tpi.Domain.Entities;

public class Doctor: EntityBase
{
    public string Name { get; private set; }
    public string? LicenseNumber { get; private set; }
    public bool Deleted { get; private set; }
    public Guid? SpecialtyId { get; set; }
    public Specialty? Specialty { get; private set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private Doctor()
    {
    }
#pragma warning restore CS8618
    #endregion

    public Doctor(string name, string? licenseNumber, Specialty specialty, Guid? id = null) : base(id)
    {
        Name = name;
        LicenseNumber = licenseNumber;
        Specialty = specialty;
        Deleted = false;
    }
    public void SoftDelete()
    {
        Deleted = true;
    }
    public void UpdateData(string name, string? licenseNumber, Specialty specialty)
    {
        Name = name;
        LicenseNumber = licenseNumber;
        Specialty = specialty;
        SpecialtyId = specialty.Id;
    }
}
