namespace Model;

public record class WeaponLoadout
{
    public required Guid PlayerId { get; set; }
    public required Guid LoadoutId { get; set; }
    public required WeaponData SemiAutoPistol { get; set; }
    public required WeaponData SuppressedPistol { get; set; }
    public required WeaponData AutoPistol { get; set; }
    public required WeaponData HighcalPistol { get; set; }
    public required WeaponData HeavyShotgun { get; set; }
    public required WeaponData AutoShotgun { get; set; }
    public required WeaponData TacticalSMG { get; set; }
    public required WeaponData RapidfireSMG { get; set; }
    public required WeaponData SuppressedSMG { get; set; }
    public required WeaponData StandardAR { get; set; }
    public required WeaponData SemiAutoAR { get; set; }
    public required WeaponData BurstAR { get; set; }
    public required WeaponData TacticalAR { get; set; }
    public required WeaponData SuppressedAR { get; set; }
    public required WeaponData HeavyAR { get; set; }
    public required WeaponData HighcalMG { get; set; }
    public required WeaponData RapidfireMG { get; set; }
    public required WeaponData SemiAutoSniper { get; set; }
    public required WeaponData BoltActionSniper { get; set; }
    public required WeaponData Melee { get; set; }
}