using System;
using System.Threading.Tasks;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using System.Linq;

namespace WadeInWater
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "WaterDamagePatcher.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            Console.WriteLine("Starting Water Damage Patcher...");
            
            int modifiedCount = 0;
            
            // Iteriere durch alle Water Records in der Load Order
            foreach (var waterContext in state.LoadOrder.PriorityOrder.Water().WinningContextOverrides())
            {
                var waterRecord = waterContext.Record;
                
                // Prüfe ob das "Causes Damage" Flag bereits gesetzt ist
                if (waterRecord.Flags?.HasFlag(Water.Flag.CausesDamage) != true)
                {
                    // Erstelle eine Kopie des Records für Modifikation
                    var modifiedWater = waterContext.GetOrAddAsOverride(state.PatchMod);
                    
                    // Setze oder erweitere die Flags
                    if (modifiedWater.Flags == null)
                    {
                        modifiedWater.Flags = Water.Flag.CausesDamage;
                    }
                    else
                    {
                        modifiedWater.Flags |= Water.Flag.CausesDamage;
                    }
                    
                    modifiedCount++;
                    Console.WriteLine($"Modified water record: {waterRecord.EditorID ?? waterRecord.FormKey.ToString()}");
                }
                else
                {
                    Console.WriteLine($"Water record already has damage flag: {waterRecord.EditorID ?? waterRecord.FormKey.ToString()}");
                }
            }
            
            Console.WriteLine($"Water Damage Patcher completed. Modified {modifiedCount} water records.");
        }
    }
}