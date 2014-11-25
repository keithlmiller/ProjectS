/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"CSVReader.cs"
 * 
 *	This script imports CSV files for use by the Speech Manager.
 *	It is based on original code by Dock at http://wiki.unity3d.com/index.php?title=CSVReader
 * 
 */

using UnityEngine;
using System.Collections;
using System.Linq; 

public class CSVReader
{

	public const string csvDelimiter = "|";
	public const string csvComma = ",";
	public const string csvTemp = "***";


	static public string[,] SplitCsvGrid (string csvText)
	{
		csvText = csvText.Replace (csvComma, csvTemp);
		csvText = csvText.Replace (csvDelimiter, csvComma);

		string[] lines = csvText.Split ("\n"[0]); 
		
		int width = 0; 
		for (int i = 0; i < lines.Length; i++)
		{
			string[] row = SplitCsvLine (lines[i]); 
			width = Mathf.Max (width, row.Length); 
		}
		
		string[,] outputGrid = new string [width + 1, lines.Length + 1]; 
		for (int y = 0; y < lines.Length; y++)
		{
			string[] row = SplitCsvLine (lines[y]);
			for (int x = 0; x < row.Length; x++) 
			{
				outputGrid[x,y] = row[x]; 
			}
		}
		
		return outputGrid; 
	}
	

	static public string[] SplitCsvLine (string line)
	{
		return (from System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(line, @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
		        System.Text.RegularExpressions.RegexOptions.ExplicitCapture)
		        select m.Groups[1].Value).ToArray();
	}
}