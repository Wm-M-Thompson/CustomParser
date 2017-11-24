using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Collections;
//using System.Text.RegularExpressions;

namespace Gelsana
{

    // I need to use a linked list of groups to keep track of them before the dictionary class is used to store the whole data



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public string updatestring(string input)
        {
            string output = input.Substring(0, Math.Min(input.Length, 70));
            if (input.Length > 80)
                return output + "...";
            else
                return input;

        }
        private void MenuItem_Click_Open(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".html"; // Default file extension
            dlg.Filter = "All documents (.*)|*.*"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                string line;
                String subline;
                try
                {
                    //Pass the file path and file name to the StreamReader constructor
                    StreamReader sr = new StreamReader(filename);

                    //Read the first line of text
                    line = sr.ReadLine();

                    //Continue to read until you reach end of file
                   // LinkedList<String> gll = new LinkedList<String>();
                    Queue gll = new Queue();

                    Dictionary<string, LinkedList> dictionary = new Dictionary<string, LinkedList>();
                    while (line != null)
                    {
                        if ((line.Length > 7) && (line.Contains("://")) && (line.Contains("\">")))
                        {

                             if (line.IndexOf("\">") != (line.LastIndexOf("\">") ))
                             {
                                 while (line.IndexOf("\">") != (line.LastIndexOf("\">") ))
                                 {
                                     string theresult = line.Substring(0, line.LastIndexOf("\">"));
                                     line = line.Substring(line.LastIndexOf("\">") + 2, line.Length - theresult.Length -2);
                                     line = theresult + " - " + line;
                                 }
                             }

                            if ((line.Contains("[/a]"))
                                ||
                            (line.Contains("[/A]")))
                            {
                                string[] words2 = line.Split('\"');

                            for (int i = 0; i < 7; i++)
                                words2[0] = words2[0].Replace(words2[0][i], char.ToLower(words2[0][i]));
 
                            // deal with huge words in the description of the line (words2[2])
                            string[] description = words2[2].Split(' ');
                            words2[2] = description[0] + " ";  // the first element is the url
                            for (int k = 1; k < description.Length; k++)
                            {
                                description[k] = updatestring(description[k]);
                                words2[2] = words2[2] + description[k] + " ";
                            }
                            
                            
                            // get the subline which will be the header of the program
                            if (words2[0].Contains("[a href"))
                            {
                                string[] words3 = words2[1].Split('/');
                                words2[2] = words2[2].Remove(0, 1);
                                words2[2] = words2[2].Replace("[/a]", " -- ");
                                subline = words3[2];
                               
                                // first, see if subline is in the dictionary
                                if (!dictionary.ContainsKey(subline))
                                {
                                    gll.Enqueue(subline);
                                    // the next two lines just sets up the passint to the dictionary class
                                    LinkedList lList = new LinkedList();
                                    lList.AddHead(words2[1],  words2[2]);
                                    dictionary.Add(subline, lList);
                                }
                                else
                                {
                                    // something like this:
                                    LinkedList lList = dictionary[subline];

                                    // divide up  words2[2]
                                    string[] stringSeparators = new string[] { " -- " };
                                    string[] result2;
                                    result2 = words2[2].Split(stringSeparators, StringSplitOptions.None);
                                    for (int k = 0; k < result2.Length; k++)
                                    {
                                        // see if we can find the item in the list
                                        if (!lList.findlNodes(words2[1], result2[k]))
                                        {
                                            lList.AddTail(words2[1], result2[k]);
                                            // now add it back
                                            dictionary[subline] = lList;
                                        }
                                        else if (!lList.findsubstring(words2[1], result2[k]))
                                        {
                                            Node cur = lList.getHead();
                                            while (cur != null)
                                            {
                                                if (words2[1] == (string)cur.index_string)
                                                {
                                                    string str_study = (string)cur.data;
                                                    if (result2[k].Length > 11)
                                                        str_study = str_study + " -- " + result2[k];
                                                    else
                                                        str_study = str_study + result2[k];
                                                    cur.data = str_study;
                                                    // now we have to put the thing back
                                                    dictionary[subline] = lList;

                                                }
                                                cur = cur.next;
                                            }
                                        }
                                    }
                                }
                            }
                            }
                        }

                        //Read the next line
                        line = sr.ReadLine();
                    }

                    //close the file
                    sr.Close();
                    // Console.ReadLine();

                    // output the file:
                    string filename2 = dlg.FileName + ".tmp";
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filename2))
                    {
                        string content;
                        while (gll.Count !=0)
                        {
                            try
                            {
                            content = (string)gll.Dequeue();
                            // take this and try to fine in the dictionary
                            LinkedList locallinkedlist = dictionary[content];
                            file.WriteLine("");
                            file.WriteLine("<h3>"+content+"</h3>");
                            Node cur = locallinkedlist.getHead();
                            
                            string[] stringSeparators = new string[] { " -- " };

                                while (cur != null)
                                {
                                    string strdata = cur.data.ToString();
                                    string[] result2;
                                    result2 = strdata.Split(stringSeparators, StringSplitOptions.None);
                                    // write it back to the output
                                    if (result2.Length == 1)
                                        strdata = result2[0] + "[/a]";
                                    else
                                        strdata = result2[0] + "[/a]" + result2[1];
                                    for (int k = 2; k < result2.Length; k++)
                                    {
                                        if (result2[k] != "")
                                        {
                                            if (result2[k].Length > 11)
                                                strdata = strdata + " -- " + result2[k];
                                            else
                                                strdata = strdata + result2[k];

                                        }
                                    }
                                    // now add the important front part
                                    strdata = "[a href=\"" + cur.index_string + "\">" + strdata;

                                    file.WriteLine(strdata);
                                    cur = cur.next;
                                }
                            }
                            catch (Exception e3)
                            {
                                Console.WriteLine("Exception: " + e3.Message);
                            }

                        }
                        file.Close();
                    }
                }
                catch (Exception e2)
                {
                    
                    Console.WriteLine("Exception: " + e2.Message);
                }
                finally
                {
                    Console.WriteLine("Executing finally block.");
                }


            }
        }
        private void MenuItem_Click_Exit(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        ///  This is the second parser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_Second_Parse(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".tmp"; // Default file extension
            dlg.Filter = "All documents (.tmp)|*.*"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                                // Open document
                string filename = dlg.FileName;
                string line;
                //String subline;
 
                Queue qleft = new Queue();
                Queue qright = new Queue();              
                
                try
                {
                    //Pass the file path and file name to the StreamReader constructor
                    StreamReader sr = new StreamReader(filename);

                    //Read the first line of text
                    line = sr.ReadLine();
                    if (line == "")
                        while (line == "")
                            line = sr.ReadLine();

                    string stringleft = "";
                    string stringright = "";

                    // set up thie first line
                    //if  (line != null)
                    //if (((line[1] == 'h') && (line[2] == '3')) == false)
                    //{
                        //while (((line[1] == 'h') && (line[2] == '3')) == false)
                        //{
                        //    line = sr.ReadLine();
                        //    if  (line != null)
                        //        break;
                        // }
                    //}

                    while (line != null)
                    {
                        #region ifstatements
                       // if ((line[1] == 'h') && (line[2] == '3'))
                       // {
                            stringleft = stringleft + line + '\n';
                            //line = sr.ReadLine();
                            while (line != "")
                            {
                            //while (((line[1] == 'h') && (line[2] == '3')) == false)
                            //{
                                line = sr.ReadLine();
                                if  (line == null)
                                    break;
                                stringleft = stringleft + line + '\n';
                            //}
                            }
                            qleft.Enqueue(stringleft);
                            stringleft = line;
                        //}
                        if  (line == null)
                                    break;
                        //if ((line[1] == 'h') && (line[2] == '3'))
                        //{
                            line = sr.ReadLine();
                            stringright = stringright + line + '\n';
                            while (line != "")//(((line[1] == 'h') && (line[2] == '3')) == false)
                            {
                                line = sr.ReadLine();
                                if  (line == null)
                                    break;
                                stringright = stringright + line + '\n';
                            }
                            qright.Enqueue(stringright);
                            stringright = line;
                        //}
                        if  (line == null)
                                    break;
                        #endregion

                        line = sr.ReadLine();

                    }
                    // LETS CLOSE THE INPUT FILE
                    sr.Close();

                    // LET'S OPEN THE OUTPUT
                    string filename2 = dlg.FileName.Replace(".tmp", ".HTML");
                    filename2 = filename2.Replace("html.HTML", ".html");
                    filename2 = filename2.Replace("htm.HTML.", ".html");

                    string outputstring = "";


                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filename2))
                    {



                        // write new output here
                        
                            file.WriteLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">");
file.WriteLine("<HTML>");
file.WriteLine("<HEAD>");
file.WriteLine("	<META HTTP-EQUIV=\"CONTENT-TYPE\" CONTENT=\"text/html; charset=windows-1252\">");
file.WriteLine("	<TITLE></TITLE>");
file.WriteLine("	<STYLE TYPE=\"text/css\">&bull; <u>");
file.WriteLine("	<!--");
file.WriteLine("		@page { margin: 0.00in }");
file.WriteLine("		P { margin-top: 0.14in; margin-bottom: 0.08in; font-family: \"Verdana\" }");
file.WriteLine("		H3 { color: #3f3f3f; font-family: \"Segoe UI\", \"Verdana\", \"Arial\" }");
file.WriteLine("		BLOCKQUOTE { font-family: \"Verdana\" }");
file.WriteLine("		A:link { color: #1364c4; text-decoration: none }");
file.WriteLine("	-->");
file.WriteLine("	</STYLE>");
file.WriteLine("</HEAD>");
file.WriteLine("<BODY LANG=\"en-US\" LINK=\"#1364c4\" BGCOLOR=\"#b0b0b0\" DIR=\"LTR\" background=\"http://gelsana.com/assets/backgrounds/lgrey091.gif\"  >");
file.WriteLine("<TABLE WIDTH=100% BORDER=1 BORDERCOLOR=\"#000000\" CELLPADDING=4 CELLSPACING=3 STYLE=\"page-break-before: always\"     background=\"http://gelsana.com/assets/backgrounds/lgrey011.jpg\" >");
//file.WriteLine("	<COL WIDTH=473*>");
//file.WriteLine("	<COL WIDTH=475*>");
file.WriteLine("	<TR VALIGN=TOP>");
file.WriteLine("		<TD WIDTH=50% BGCOLOR=\"#e6e6e6\" background=\"http://gelsana.com/assets/backgrounds/lgrey064.jpg\">");
                        while (qleft.Count !=0)
                        {

                            outputstring = (string)qleft.Dequeue();
                            outputstring = outputstring.Replace("</h3>\n", "</h3>\n<blockquote>\n<");
                            outputstring = outputstring.Replace("<[", "<");
                            outputstring = outputstring.Replace("[a href", "<br><a href");
                            outputstring = outputstring.Replace("\">", "\">&#x95 <u>");
                            outputstring = outputstring.Replace("[/a]", "</u></a>");
                            outputstring = outputstring.Replace("[/A]", "</u></a>");
                            outputstring = outputstring.Replace("\n\n", "\n");
                            file.Write(outputstring);
                            file.WriteLine("</blockquote>");
                        }


                        file.WriteLine("	    </TD>");
                        //file.WriteLine("    </TR>");
                        //file.WriteLine("	<TR VALIGN=TOP>");
                        file.WriteLine("		<TD WIDTH=50% BGCOLOR=\"#e6e6e6\" background=\"http://gelsana.com/assets/backgrounds/lgrey064.jpg\">");

                        while (qright.Count != 0)
                        {

                            outputstring = (string)qright.Dequeue();
                            outputstring = outputstring.Replace("</h3>\n", "</h3>\n<blockquote>\n<");
                            outputstring = outputstring.Replace("<[", "<");
                            outputstring = outputstring.Replace("[a href", "<br><a href");
                            outputstring = outputstring.Replace("\">", "\">&#x95 <u>");
                            outputstring = outputstring.Replace("[/a]", "</u></a>");
                            outputstring = outputstring.Replace("[/A]", "</u></a>");
                            outputstring = outputstring.Replace("\n\n", "\n");
                            file.Write(outputstring);
                            file.WriteLine("</blockquote>");
                        }
                        file.WriteLine("	    </TD>");
                        file.WriteLine("    </TR>");
                        file.WriteLine("</TABLE>");
                        file.WriteLine("</BODY>");
                        file.WriteLine("</HTML>");
                    
                        file.Close();

                    }
                }
                catch (Exception e5)
                {
                    Console.WriteLine("Exception: " + e5.Message);
                }

            }
        }
    }
}
