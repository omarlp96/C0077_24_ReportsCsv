using FTOptix.Core;
using FTOptix.NetLogic;
using System.IO;
using System.Text;
using System;
using UAManagedCore;
using System.Runtime.InteropServices;

public class ExportCsv : BaseNetLogic
{
    private IUAVariable esitoVariable;
    private IUAVariable progressivoVariable;
    private IUAVariable altezzaTaglioVariable;
    private IUAVariable profonditaTaglioVariable;
    private IUAVariable rimanenzaTaglioVariable;
    private IUAVariable larghezzaTotVariable;
    private IUAVariable angoloTaglioVariable;

    public override void Start()
    {
        // Inicializa las variables
        esitoVariable = LogicObject.GetVariable("esito");
        progressivoVariable = LogicObject.GetVariable("progressivo");
        altezzaTaglioVariable = LogicObject.GetVariable("altezzaTaglio");
        profonditaTaglioVariable = LogicObject.GetVariable("profonditaTaglio");
        rimanenzaTaglioVariable = LogicObject.GetVariable("rimanenzaTaglio");
        larghezzaTotVariable = LogicObject.GetVariable("larghezzaTot");
        angoloTaglioVariable = LogicObject.GetVariable("angoloTaglio");
    }

    [ExportMethod]
    public void Export()
    {
        try
        {
             var csvPath = GetCSVFilePath();
            
            if (string.IsNullOrEmpty(csvPath))
                throw new Exception("No CSV file chosen, please fill the CSVPath variable");
           
                

            char? fieldDelimiter = GetFieldDelimiter();
            bool wrapFields = GetWrapFields();

            // Obtener los valores de las variables, asegurándonos de que no sean nulos
            string var1 = esitoVariable?.Value;
            string var2 = progressivoVariable.Value;
            string var3 = altezzaTaglioVariable?.Value;
            string var4 = profonditaTaglioVariable?.Value;
            string var5 = rimanenzaTaglioVariable?.Value;
            string var6 = larghezzaTotVariable?.Value;
            string var7 = angoloTaglioVariable?.Value;
           // Log.Info("Valor angolo" + angoloTaglioVariable.Value);

            // Encabezados (esto dependerá de las variables que quieras escribir)
            string[] header = new string[] { "Esito"," Progressivo","Altezza Taglio","Profondita Taglio", "Rimanenza Taglio", "Larghezza Tot", "Angolo Taglio" };

            // Escribir los resultados en el archivo CSV
            using (var csvWriter = new CSVFileWriter(csvPath) { FieldDelimiter = fieldDelimiter.Value, WrapFields = wrapFields })
            {
                csvWriter.WriteLine(header);
                WriteVariablesToCSV(var1, var2, var3, var4, var5, var6, var7, csvWriter);
            }

            Log.Info("DataLoggerExporter", "Las variables han sido exportadas exitosamente a " + csvPath);
        }
        catch (Exception ex)
        {
            Log.Error("DataLoggerExporter", "No se pudo exportar los datos: " + ex.Message);
        }
    }

    private void WriteVariablesToCSV(string var1, string var2, string var3, string var4, string var5, string var6, string var7, CSVFileWriter csvWriter)
    {
        // Escribir una fila con los valores de las variables
        var currentRow = new string[] { var1, var2, var3, var4, var5, var6, var7 };
        csvWriter.WriteLine(currentRow);
    }

    private string GetCSVFilePath()
    {
        var csvPathVariable = LogicObject.GetVariable("CSVPath");
        if (csvPathVariable == null)
            throw new Exception("CSVPath variable not found");

        return new ResourceUri(csvPathVariable.Value).Uri;
    }

    private char GetFieldDelimiter()
    {
        var separatorVariable = LogicObject.GetVariable("FieldDelimiter");
        if (separatorVariable == null)
            throw new Exception("FieldDelimiter variable not found");

        string separator = separatorVariable.Value;

        if (separator == string.Empty)
            throw new Exception("FieldDelimiter variable is empty");

        if (separator.Length != 1)
            throw new Exception("Wrong FieldDelimiter configuration. Please insert a single character");

        if (!char.TryParse(separator, out char result))
            throw new Exception("Wrong FieldDelimiter configuration. Please insert a char");

        return result;
    }

    private bool GetWrapFields()
    {
        var wrapFieldsVariable = LogicObject.GetVariable("WrapFields");
        if (wrapFieldsVariable == null)
            throw new Exception("WrapFields variable not found");

        return wrapFieldsVariable.Value;
    }

    #region CSVFileWriter
    private class CSVFileWriter : IDisposable
    {
        public char FieldDelimiter { get; set; } = ',';

        public char QuoteChar { get; set; } = '"';

        public bool WrapFields { get; set; } = false;

        public CSVFileWriter(string filePath)
        {
            streamWriter = new StreamWriter(filePath, false, System.Text.Encoding.UTF8);
        }

        public void WriteLine(string[] fields)
        {
            var stringBuilder = new StringBuilder();

            for (var i = 0; i < fields.Length; ++i)
            {
                if (WrapFields)
                    stringBuilder.AppendFormat("{0}{1}{0}", QuoteChar, EscapeField(fields[i]));
                else
                    stringBuilder.AppendFormat("{0}", fields[i]);

                if (i != fields.Length - 1)
                    stringBuilder.Append(FieldDelimiter);
            }

            streamWriter.WriteLine(stringBuilder.ToString());
            streamWriter.Flush();
        }

        private string EscapeField(string field)
        {
            var quoteCharString = QuoteChar.ToString();
            return field.Replace(quoteCharString, quoteCharString + quoteCharString);
        }

        private StreamWriter streamWriter;

        #region IDisposable Support
        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
                streamWriter.Dispose();

            disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }

    [ExportMethod]
    public void DeleteFile()
    {
        // Prendi la Path
        string filePath = GetCSVFilePath();

        //Verifichiamo se essiste
        if (File.Exists(filePath)) 
        {
            try
            {
                // Prova cancellare il file
                File.Delete(filePath);
            }
            catch (IOException ioEx)
            {
                Log.Error("Errore in cancellare il file (Bloccato o in uso): " + ioEx.Message);
            }
            catch (Exception ex)
            {
                Log.Error("Error in cancellare il file: " + ex.Message);
            }
        }
        else
        {
            Log.Error("Il file non essiste: " + filePath);
        }
    }

    #endregion
}
