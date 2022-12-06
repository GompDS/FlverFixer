using DS3FlverFixer.Util;
using SoulsAssetPipeline.FLVERImporting;
using SoulsFormats;

namespace DS3FlverFixer;

internal static class Program
{
    public static void Main(string[] args)
    {
        if (!BND4.Is(args[0])) throw new ArgumentException("Only BND4 is supported.");
        BND4 bnd = BND4.Read(args[0]);
        
        Game game = new(bnd);

        foreach (BinderFile flverFile in bnd.Files.Where(x => x.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase)))
        {
            FLVER2 flver = FLVER2.Read(flverFile.Bytes);
            flver.BufferLayouts = new List<FLVER2.BufferLayout>();
            flver.GXLists = new List<FLVER2.GXList>();

            List<FLVER2.Material> distinctMaterials = flver.Materials.DistinctBy(x => x.MTD).ToList();
            foreach (var distinctMat in distinctMaterials)
            {
                FLVER2.GXList gxList = new();
                gxList.AddRange(game.MaterialInfoBank
                    .GetDefaultGXItemsForMTD(Path.GetFileName(distinctMat.MTD).ToLower()));

                if (flver.IsNewGxList(gxList))
                {
                    flver.GXLists.Add(gxList);
                }
			    
                foreach (var mat in flver.Materials.Where(x => x.MTD.Equals(distinctMat.MTD)))
                {
                    mat.GXIndex = flver.GXLists.Count - 1;
                }
            }
            
            foreach (FLVER2.Mesh? mesh in flver.Meshes)
            {
                FLVER2MaterialInfoBank.MaterialDef? matDef = game.MaterialInfoBank.MaterialDefs.Values
                    .FirstOrDefault(x => x.MTD.Equals(
                        $"{Path.GetFileName(flver.Materials[mesh.MaterialIndex].MTD).ToLower()}"));

                if (matDef == null)
                {
                    throw new KeyNotFoundException(Path.GetFileName(flver.Materials[mesh.MaterialIndex].MTD) + " could not be found in the material info bank.");
                }

                List<FLVER2.BufferLayout> bufferLayouts = matDef.AcceptableVertexBufferDeclarations[0].Buffers;
                
                mesh.Vertices = mesh.Vertices.Select(x => x.Pad(bufferLayouts)).ToList();
                List<int> layoutIndices = flver.GetLayoutIndices(bufferLayouts);
                mesh.VertexBuffers = layoutIndices.Select(x => new FLVER2.VertexBuffer(x)).ToList();
            }
            
            flverFile.Bytes = flver.Write();
        }
            
        bnd.Write(args[0], game.Compression);
    }
}