using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GPL2015_Assembler
{
    public partial class Form1 : Form
    {
        public String filepath;
        Output output;
        string outstringval;
        public List<String> opcodes = new List<string>();
        public List<String> AssemblyCodes = new List<string>();
        public List<String> Labels = new List<string>();
        public List<String> lines = new List<string>();
        public List<String> outputlines = new List<string>();


        public List<String> labelname = new List<string>();
        public List<String> labelindex = new List<string>();
        public List<int> inputToOutput = new List<int>();



        public Form1()
        {
            InitializeComponent();
            output = new Output();
            output.Show();
            output.Text = "Output";
            operations.ScrollBars = ScrollBars.Vertical;
            operations.WordWrap = false;
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);

            

            configdownload();
            InputText.AcceptsTab = true;
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                filepath = file;
                Inputlabel.Text ="Input - " + file;
                String[] linee = System.IO.File.ReadAllLines(file);
                InputText.Text = null;
                foreach(String linea in linee)
                {
                    InputText.Text = InputText.Text  + linea + System.Environment.NewLine; 
                    
                }
            }

        }
        

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (filepath != null)
            {
                System.IO.StreamWriter writer = new System.IO.StreamWriter(filepath);
                writer.Write(InputText.Text);
                writer.Close();
                writer.Dispose();
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();

                saveFileDialog.Filter = "GPL2015 Assembler (*.asm)|*.asm|txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 0;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    System.IO.StreamWriter writer = new System.IO.StreamWriter(saveFileDialog.FileName);
                    writer.Write(InputText.Text);
                    writer.Close();
                    writer.Dispose();
                }
            }
        }
        
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                openconfig(openFileDialog1.FileName);
            }
        }

        void openconfig(String filepath)
        {
            try
            {
                String[] linee = System.IO.File.ReadAllLines(filepath);
                operations.Text = null;
                operations.Text = "OpCode | Assembly" + Environment.NewLine;
                foreach (String linea in linee)
                {
                    String Opcode = linea.Substring(0, linea.IndexOf(' '));
                    String Assembly = linea.Remove(0, linea.IndexOf(' ') + 1);
                    opcodes.Add(Opcode);
                    Assembly = Regex.Replace(Assembly, @"\s", "");
                    AssemblyCodes.Add(Assembly);
                    Console.Write(opcodes.Count + "lungo");
                    operations.Text = operations.Text + Opcode + " | " + Assembly + System.Environment.NewLine;

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
            }
        }

        public void Preload()
        {
            int cont = -1;
            string[] inputstrings = InputText.Text.Split('\n');
            foreach(string linea in inputstrings)
            {
                cont++;
                string lineanormale = Regex.Replace(linea, @"\s", "");


                var special = specialloop(lineanormale);
                if (special.Item1)
                {
                    output.ShowMessage("trovata etichetta alla linea, " + cont);
                }
            }
        }

        public int countline()
        {
            int lines = outputBox.Lines.Count();
            if (lines > 0)
            {
                return lines -= String.IsNullOrWhiteSpace(outputBox.Lines.Last()) ? 1 : 0;

            }
            else
            {
                return 0;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            lines.Clear();
            Labels.Clear();
            outputlines.Clear();
            labelname.Clear();
            labelindex.Clear();
            inputToOutput.Clear();

            Preload();
            output.ShowMessage("---Avvio compilazione" + DateTime.Now.Hour + ":" + DateTime.Now.Minute + "--- Modalità senza esecuzione");
            
            compile();
        }
        
        public String resolvelabel(String lab)
        {
            int indice = -1;
            indice = Labels.FindIndex(x => x.Equals(lab));
            if(indice != -1){
                int indi = Int32.Parse(lines[indice]);
                try
                {

                    int addr = int.Parse(outputlines[indi]);
                    string bin = Convert.ToString(addr, 2);

                    //String addrbin = Convert.ToString(addr, 2);
                    return bitconvert(bin);
                }
                catch (System.ArgumentOutOfRangeException)
                {
                    return lab + "JUMP";
                }
            }
            else
            {
                return "xxxx" + lab;
            }
        }

        

        public bool specialLoad(String operation)
        {
            if (operation.Contains("LDA,"))
            {
                String load = operation.Replace("LDA,", "");
                //string bin = Convert.ToString(int.Parse(load), 2);

                if (load != "B" && load != "C")
                {
                    int res = 0;
                    return int.TryParse(load, out res);
                }
                else { return false; }
            }
            else
            {
                return false;
            }
        }

        public String bitconvert(String bin) {

            switch (bin.Length)
            {
                case 1:
                    return "000" + bin;
                case 2:
                    return "00" + bin;
                case 3:
                    return "0" + bin;
                case 4:
                    return bin;
            }

            return bin;
        
        }


        public bool specialJPP(string operation)
        {
            if (operation.Contains("JPP,"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

       
        public Tuple<bool, String> specialloop(String operation)
        {
            if (operation.Contains(":"))
            {
                operation = operation.Replace(":", "");
                return Tuple.Create(true, operation);

            }
            else
            {
                return Tuple.Create(false, "");
            }
        }

        public bool specialJPM(string operation)
        {
            if (operation.Contains("JPM,"))
            {
                /*
                String load = operation.Replace("JPM,", "");
                if (load != "B" && load != "C")
                {
                    return System.Text.RegularExpressions.Regex.IsMatch(load, @"\A\b[0-9a-fA-F]+\b\Z");
                }
                else { return false; }*/
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool specialJp(String operation)
        {
            if (operation.Contains("JP"))
            {
                if(operation.Contains("JPP,")|| operation.Contains("JPM,"))
                {
                    return false;
                }
                return true;

            }
            else
            {
                return false;
            }
        }


        public string containsasscode(string linea)
        {
            foreach (String asscode in AssemblyCodes)
            {

                string asscodenormale = Regex.Replace(asscode, @"\s", "");
                if (asscodenormale == linea)
                {
                    return asscodenormale;
                }
            }
            return "no";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 0;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string[] outstrings = outputBox.Text.Split('\n');
                String[] towrite = new String[2];
                towrite[0] = "v2.0 raw";
                outstringval = "";
                foreach (String outstring in outstrings)
                {
                    string outnormale = Regex.Replace(outstring, @"\s", "");

                    if (outnormale != null && !outnormale.Equals("") && !outnormale.Equals(" "))
                    {
                        execute(outnormale);
                        string hexval = HexConverted(outnormale);
                        Console.WriteLine(hexval);
                        if (!hexval.Equals("errore di compilazione"))
                        {
                            outstringval = outstringval + hexval + " ";
                        }

                    }
                }
                towrite[1] = outstringval;
                System.IO.File.WriteAllLines(saveFileDialog.FileName, towrite);
                Console.WriteLine("salvato");
            }
            
        }




        public void postcompile()
        {
            int index = 0;
           foreach(String lb in labelname)
            {
                int ind = int.Parse(labelindex[index]);
                string bin = Convert.ToString(ind, 2);
                bin = bitconvert(bin);
                outputBox.Text = outputBox.Text.Replace("xxxx" + lb, bin);
                    index++;
            }
        }

        public void compile()
        {
            int cont = -1;
            outputBox.Text = null;
            string[] inputstrings = InputText.Text.Split('\n');
            foreach (String linea in inputstrings)
            {
                cont++;
                outputlines.Add(countline().ToString());
                int cnt = countoutline();
               // output.ShowMessage(cont + " linea " + cnt);

                inputToOutput.Add(cnt);



                string asscode;
                string lineanormale = Regex.Replace(linea, @"\s", "");
                var spec = specialloop(lineanormale);
                if (spec.Item1)
                {
                    output.ShowMessage("etichetta " + spec.Item2 + " si apre alla linea" + cnt);
                    labelname.Add(spec.Item2);
                    labelindex.Add(cnt.ToString());
                }

                lineanormale = removecomments(lineanormale);

               

                    if (specialLoad(lineanormale)|| specialJp(lineanormale) || specialJPP(lineanormale) || specialJPM(lineanormale) )
                {
                    execute(lineanormale);
                }
                else
                {
                    asscode = containsasscode(lineanormale);
                    if (asscode.Equals("INA"))
                    {
                        execute(lineanormale);
                    }
                    else if (asscode != "no")
                    {
                        execute(lineanormale);
                        int indice = AssemblyCodes.FindIndex(x => x.StartsWith(asscode));
                        outputBox.Text = outputBox.Text + opcodes[indice] + Environment.NewLine;
                    }
                }

            }

            postcompile();
        }

        private int countoutline()
        {
            string[] outstring = outputBox.Text.Split('\n');
            int ct = -1;
            foreach (String linea in outstring)
            {
                ct++;
            }

            return ct;
        }

        public bool checkbinary(String str)
        {
            foreach(char c in str)
            {
                if(c != '0' && c != '1')
                { return false; }
            }

            return true;
        }

        public String onecomplement(String bin)
        {
            String ret = "";
            foreach(char c in bin)
            {
                if (c == '0')
                {
                    ret = ret + "1";
                }
                else
                {
                    ret = ret + "0";
                }
            }

            return ret;
        }

        public void execute (string operation)
        {
            if(opcodes.Count != 0){
                output.ShowMessage("Provo ad eseguire " + operation);
                if (specialLoad(operation))
                {
                    output.ShowMessage("trovata opzione speciale con fattore " + operation.Replace("LDA,", ""));

                    int indice = AssemblyCodes.FindIndex(x => x.StartsWith("LDA,n"));
                    outputBox.Text = outputBox.Text + opcodes[indice] + Environment.NewLine;
                    
                    if(int.Parse(operation.Replace("LDA,", "")) < 0)
                    {
                        String num = operation.Replace("LDA,", "");
                        num = num.Replace("-", "");
                        string bin = Convert.ToString(int.Parse(num), 2);
                        bin = onecomplement(bitconvert(bin));

                        int dec = Convert.ToInt32(bin, 2);
                        dec++;
                        String final = Convert.ToString(dec, 2);
                        outputBox.Text = outputBox.Text + final + Environment.NewLine;

                    }
                    else if (checkbinary(operation.Replace("LDA,", "")))
                    {
                        outputBox.Text = outputBox.Text + bitconvert(operation.Replace("LDA,", "")) + Environment.NewLine;
                    }
                    else
                    {
                        string bin = Convert.ToString(int.Parse(operation.Replace("LDA,", "")), 2);
                        outputBox.Text = outputBox.Text + bitconvert(bin) + Environment.NewLine;
                    }

                }
                else if (specialJp(operation))
                {
                    output.ShowMessage("trovato jump con indirizzo " + operation.Replace("JP", ""));

                    int indice = AssemblyCodes.FindIndex(x => x.StartsWith("JP"));
                    outputBox.Text = outputBox.Text + opcodes[indice] + Environment.NewLine;

                    outputBox.Text = outputBox.Text + resolvelabel(operation.Replace("JP", "")) + Environment.NewLine;
                }
                else if (specialJPP(operation))
                {
                    output.ShowMessage("trovato jump positivo con indirizzo " + operation.Replace("JPP,", ""));

                    int indice = AssemblyCodes.FindIndex(x => x.StartsWith("JPP,"));
                    outputBox.Text = outputBox.Text + opcodes[indice] + Environment.NewLine;

                    outputBox.Text = outputBox.Text + resolvelabel(operation.Replace("JPP,", "")) + Environment.NewLine;
                }
                else if (specialJPM(operation))
                {
                    output.ShowMessage("trovato jump negativo con indirizzo " + operation.Replace("JPM,", ""));

                    int indice = AssemblyCodes.FindIndex(x => x.StartsWith("JPM,"));
                    outputBox.Text = outputBox.Text + opcodes[indice] + Environment.NewLine;

                    outputBox.Text = outputBox.Text + resolvelabel(operation.Replace("JPM,", "")) + Environment.NewLine;
                }
                else
                {
                    switch (operation)
                    {
                        case "HALT":
                            break;
                        case "LDA,B":
                            break;
                        case "LDA,C":
                            break;
                        case "LDB,A":
                            break;
                        case "LDC,A":
                            break;
                        case "ADDA,B":
                            addab();
                            break;
                        case "ADDA,C":
                            addac();
                            break;
                        case "INA":
                            ina();
                            break;
                        case "SUBA,C":
                            subac();
                            break;
                        case "INCB":
                            break;
                        case "OUTA":
                            break;
                        case "DECB":
                            break;
                    }
                }
            }else{
                MessageBox.Show("Non hai caricato nessuna configurazione", "Errore di configurazione");
            }
            
        }

        
       

        public String removecomments(String inp)
        {
            int indic = inp.LastIndexOf(";");
            if (indic > 0)
            {
                return inp = inp.Substring(0, indic);
            }
            return inp;
        }

        void ina()
        {
          
                int indice = AssemblyCodes.FindIndex(x => x.StartsWith("INA"));
                outputBox.Text = outputBox.Text + opcodes[indice] + Environment.NewLine;
            
        }

       

        void addab()
        {
            //RegA = RegA + RegB;
        }

        void addac()
        {
           

            //RegA = RegA + RegC;
        }

        void subac()
        {
            

            // RegA = RegA - RegC;
        }

        public int toint(String number)
        {
            return Convert.ToInt32(number, 2);
        }


        void Jpaddr(string operation)
        {
               
           
            
        }
        


        

        string HexConverted(string strBinary)
        {
            try
            {
                string strHex = Convert.ToInt32(strBinary, 2).ToString("X");
                return strHex;
            }catch(Exception )
            {
                return "errore";
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            configdownload();
        }

        private void Inputlabel_Click_1(object sender, EventArgs e)
        {
            loadfile();
        }

        public void loadfile()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "c:\\";
            openFileDialog1.Filter = "GPL2015 Assembler (*.asm)|*.asm|txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog1.FilterIndex = 0;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {

                    filepath = openFileDialog1.FileName;
                    String[] linee = System.IO.File.ReadAllLines(openFileDialog1.FileName);
                    InputText.Text = "";
                    foreach (String linea in linee)
                    {
                        InputText.Text = InputText.Text + linea + System.Environment.NewLine;


                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        void configdownload()
        {

            String url = "https://raw.githubusercontent.com/AlessioGiacobbe/Microprocessor-2017/master/config";
                     
            WebClient myWebClient = new WebClient();
            myWebClient.DownloadFile(url, Path.GetTempPath() + "config");

            openconfig(Path.GetTempPath() + "config");
            
        }

        

        

        private void detectchange(object sender, EventArgs e)
        {
            int currentline = InputText.GetLineFromCharIndex(InputText.SelectionStart);
            linecount.Text = "Linea : " + currentline;
           

            if (inputToOutput.Count > currentline)
            {
                outputBox.SelectAll();
                outputBox.SelectionColor = Color.Black;
                int charindex = outputBox.GetFirstCharIndexFromLine(inputToOutput[currentline]);
                
                outputBox.Select(charindex, 4);
                

                outputBox.SelectionColor = Color.Blue;

            }

        }

        private void carica_Click(object sender, EventArgs e)
        {
            loadfile();
        }
    }
}
