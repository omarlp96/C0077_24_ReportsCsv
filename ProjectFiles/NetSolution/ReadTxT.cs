#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
using FTOptix.S7TiaProfinet;
using FTOptix.Retentivity;
using FTOptix.CoreBase;
using FTOptix.CommunicationDriver;
using FTOptix.Core;
using System.IO;
using System.Threading;
using FTOptix.OPCUAServer;
using FTOptix.Recipe;
#endregion

public class ReadTxT : BaseNetLogic
{
    private IUAVariable filePath;
   // private IUAVariable ReadOK;
    private IUAVariable triggerRead;
    private IUAVariable recipeNum;
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        filePath = Project.Current.GetVariable("Model/PathTxt");
       // ReadOK = Project.Current.GetVariable("Model/ReadOk");
        triggerRead = Project.Current.GetVariable("Model/ReadLine");
        recipeNum = Project.Current.GetVariable("Model/RecipeNum");
    }
    [ExportMethod]
    public void ReadLine()
    {
        string path = filePath.Value;
        try
        {
            // Read all lines from file into an array of strings
            string[] lines = File.ReadAllLines(path);

            // Check if the file is not empty
            if (lines.Length > 0)
            {
                // Obtener la última línea
                string lastline = lines[lines.Length - 1];
                LogicObject.GetVariable("lastline").Value = lastline;
                //  recipeNum.Value = lastline.ToString();
                if (int.TryParse(lastline, out int parsedValue))
                {
                    recipeNum.Value = parsedValue;
                }
                else
                {
                    Log.Error($"No se pudo convertir '{lastline}' a número.");
                }
                Log.Info("Leyendo");
                Thread.Sleep(200);
                //ReadOK.Value = true;
                Thread.Sleep(200);
               // ReadOK.Value = false;
                triggerRead.Value = false;

            }
            else
            {
                Log.Error("The file is empty");
            }
        }
        catch (FileNotFoundException ex)
        {
            Log.Error("File not found", ex.Message);
        }
        catch (IOException ex)
        {
            Log.Error("Problem reading the file", ex.Message);
        }
        catch (Exception ex)
        {
            Log.Error("aqui", ex.Message);
        }
    }
}
