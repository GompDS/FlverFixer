using SoulsAssetPipeline.FLVERImporting;
using SoulsFormats;

namespace DS3FlverFixer;

public class Game
{
    /// <summary>
    /// DCX compression used by this game.
    /// </summary>
    public DCX.Type Compression { get; }
    /// <summary>
    /// Material Info Bank used by this game.
    /// </summary>
    public FLVER2MaterialInfoBank MaterialInfoBank { get; }

    public Game(IBinder bnd)
    {

        string cwd = AppDomain.CurrentDomain.BaseDirectory;
        
        if (bnd.Files.Any(x => x.Name.Contains(@"N:\FDP\data\INTERROOT_win64\")))
        {
            MaterialInfoBank = FLVER2MaterialInfoBank.ReadFromXML($"{cwd}Res\\BankDS3.xml");
            Compression = DCX.Type.DCX_DFLT_10000_44_9;
        }
        else if (bnd.Files.Any(x => x.Name.Contains(@"N:\NTC\data\Target\INTERROOT_win64")))
        {
            MaterialInfoBank = FLVER2MaterialInfoBank.ReadFromXML($"{cwd}Res\\BankSDT.xml");
            Compression = DCX.Type.DCX_DFLT_11000_44_9;
        }
        else if (bnd.Files.Any(x => x.Name.Contains(@"N:\GR\data\INTERROOT_win64")))
        {
            MaterialInfoBank = FLVER2MaterialInfoBank.ReadFromXML($"{cwd}Res\\BankER.xml");
            Compression = DCX.Type.DCX_DFLT_11000_44_9;
        }
        else
        {
            throw new ArgumentException("Binder is not from a supported game.");
        }
    }

}