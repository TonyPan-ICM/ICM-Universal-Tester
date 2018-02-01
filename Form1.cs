/*'
 * 
done	1.	Bug in software:
			a.	Occurs when probes are not ALL assign, or a gap in probes assignment
			b.	Sometimes when assigning probes they are shown out of order
			***Bug reason: map according to user input***

2.	Improvements needed:
	a.	The Detail Frame should show all probes all at once, most cases you will have to scroll down
	***Need to expand to show 12 rows

	b.	Board On Tester Assignment, so we don’t have to edit the XML to add boards
	***Allow to add boards and rearrange boards

done	c.	Debug mode:
		i.	Where a wait statement is inserted between every line in the script
	***Purpose: to execute steps manually one at a time.
	***When enabled, add every wait() in every line

done	d.	Enable/Disable:
		i.	Where each statement has a check box rather to enable this state or disable this statement, by default it should be enabled
	***Purpose: to enable and disable test steps at will.
	***Solution: Still in script but can be read by builder but should not be able to read by Pi.

done	e.	Every Card should be readable
		i.	At the moment only the AC card is readable, but every card should be readable;
			ie I can read from the power board and find the status of the power relays
		***Currently only the AC Reading board has the "Verify Output" command. Would like to have other boards having the same (but call it "Verify Input".)
 
done	3.	Scan Barcode:
			a.	This needs to be fix, as it does not allow the user to pick a card. It should function like setting a relay: setoutputs
			***Currently there is no GUI to update the command step

done	4.	UTV1.5 - Update AC Reading card to 12 channels
			***Update the board's pin number to 12

done	5.	UTV1.5 - Update Relay card to 12 channels
			***Update the board's pin number to 12

done	6.	UTV1.5 – Add PS4 as an independent open to the Power card
		***Power board has 3 pins now. The new 4th pin is called "PS4". Value is 1 or 0, same with hot/neutral reverse.

done	Change "Simulate input" to "Set tester output"
		Change "Verify output" to "Verify tester input"
 
*/



/*
	Add/Remove/Edit currentBoard board placement order
	Support ADC value verification
	Support multiple out on the same fixture
	Support wifi MAC programming and web registration / verification

1.       
you add “BarcodeScan” to your UTF Windows Software, the option would be the same as  “Simulate Inputs”.
on this command the RPI will perform barcode trigger, decode the serial #, ect…
 
<Step ID="0001">
    <CommandCodeName>SI</CommandCodeName>
    <_Command>BarcodeScan</_Command>
    <BoardId>4</BoardId>
    <_Board>RelayBoard</_Board>
    <_BoardAddr>4</_BoardAddr>
    <Probe ID="01">
      <Name>Flame Sim Enable</Name>
      <BoardId>4</BoardId>
      <ChannelId>1</ChannelId>
      <Value>0</Value>
    </Probe>
  </Step>
 
2.       
change “Repeat Start” to accept 0 as an option. When set to 0, the RPI will prompt the user to enter the number of repeats
 
3.            
Added  UartRead script command, passing values are integer representing constants strings withing the RASPI CODE
This command reads the uart until some matching string is found
 
<Step ID="0005">
    <CommandCodeName>SI</CommandCodeName>
    <_Command>UartRead</_Command>
    <BoardId>6</BoardId>
    <_Board>Serial Programmer</_Board>
    <_BoardAddr>6</_BoardAddr>
    <_Const>1</_Const>
    <TestStepNote>reading uart</TestStepNote>
  </Step>
 
4. added UartSearch script command, passing values are integer representing constants string within
This command searches the uart buffer for some matching string
 
5. added WaitForImageInstr script command, the passing values are integer representing an index within the Image Folder,
This command loads an image that has instructions for the user to perform
 
 * 
 * 
 * 
 */


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Xml.Xsl;
 

using System.Data.SqlClient;

using System.Drawing.Drawing2D;
using System.Collections;
using System.Drawing.Imaging;
using System.IO.IsolatedStorage;

using System.Reflection;     // to use Missing.Value

using System.Threading;
using System.Diagnostics;
using Microsoft.Office.Interop.Excel;

namespace ICM_Universal_Tester
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			string stringPathFiles = GetParentFolder(System.Windows.Forms.Application.StartupPath, 0) + constStringFolderFiles;

			stringNewTestFile = stringPathFiles + constStringNewTestFile;
			stringTesterConfigFile = stringPathFiles + constStringTesterConfigFile;
			stringModulesFile = stringPathFiles + constStringModulesFile;

			if (stringModulesFile.Length > 0 && File.Exists(stringModulesFile))
			{
				modulesXmlDoc = XDocument.Load(stringModulesFile);	// load it
			}
			else
			{
				modulesXmlDoc = null;
			}
			if (modulesXmlDoc == null)
			{
				MessageBox.Show("Error: Module file not Found.");
			}
			/*
			if (stringTesterConfigFile.Length > 0 && File.Exists(stringTesterConfigFile))
			{
				testerConfigXmlDoc = XDocument.Load(stringTesterConfigFile);	// load it
			}
			else
			{
				testerConfigXmlDoc = null;
			}
			if (testerConfigXmlDoc == null)
			{
				MessageBox.Show("Error: Test config file not Found.");
			}
			 * */
			string cmdAlias = "";
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_SetTesterOutput); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_VerifyTesterInput); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_Wait); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_WaitRandom); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_WaitForTesterInputs); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_SubScript); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_RepeatStart); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_RepeatEnd); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_WaitOutput_Camera); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_WaitUser); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_WaitImage); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_DebugWaitUser); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_ReadUart); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_SearchUart); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_TransmitUart); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_BarcodeScan); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			//cmdAlias = SearchCommandAliasByCodeName(stringCommand_BarcodeScan_Old); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_FlashRenesas); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_FlashMicrochip); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_FlashCypress); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_FlashInventek); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_EraseRenesas); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			cmdAlias = SearchCommandAliasByCodeName(stringCommand_EraseCypress); if (cmdAlias.Length > 0) comboBoxTestType.Items.Add(cmdAlias);
			
			/*
			for (int i = 1; ; i++)
			{
				XElement currentBoard = SearchModuleByPosition(i.ToString());
				if (currentBoard != null)
				{
					string name = GetBoardName(currentBoard.Element(stringModuleAlias).Value, i.ToString());
					comboBoxBoard.Items.Add(name);
				}
				else
				{
					break;
				}
			}
			 * */
			//MessageBox.Show("New features have been implemented:\n\n\n\n\n\n1. New command \"Wait for a random time.\"\n\n2. You now can set wait time up to 20 days. (Previously 9 hours.)\n\n3. You now can call \"SubScripts\" from a script. Subscripts have to be located in the same folder.\n\n4. You can make the script to run to the end despite failures.\n\n\n\n\n\nClick the new red buttons to use these features.", "ICM Universal Tester Test Script Editor");
			MessageBox.Show("New in this version:\n\n\n\n\n\n1. Show existing test options at the bottom of the test step screen when a test script is loaded.\n\n2. Make the probe list longer to show more pins.\n\n\n\n\n\n", "ICM Universal Tester Test Script Editor Release Note");
		}

		const string stringModule = "Module";		// attributes
		const string stringModuleCodeName = "ModuleCodeName";	// element
		const string stringModuleAlias = "ModuleAlias";	// element
		const string stringModuleTotalChannelsIn = "TotalChannelsIn";	// element
		const string stringModuleTotalChannelsOut = "TotalChannelsOut";	// element
		const string stringSpecialFlameBoardAdditionalChannels = "SpecialFlameBoardAdditionalChannels";	// element
		const string stringSpecialPowerBoardAdditionalChannels = "SpecialPowerBoardAdditionalChannels";	// element
		const string stringSpecialProgrammingBoardAdditionalChannels = "SpecialProgrammingBoardAdditionalChannels";	// element
		const string stringIoDirection = "IoDirection";	// element
		const string stringApplicableCommand = "ApplicableCommand";	// atribute
		const string stringPin = "Pin";	// atribute
		const string stringPinAlias = "PinAlias";
		const string stringReferencePin = "ReferencePin";

		const string stringCommandCodeName = "CommandCodeName";	// element
		const string stringCommandAlias = "CommandAlias";	// element

		const string stringCommand_SetTesterOutput = "SI";
		const string stringCommand_BarcodeScan = "BS";
		const string stringCommand_BarcodeScan_Old = "B";
		const string stringCommand_VerifyTesterInput = "VO";
		const string stringCommand_WaitRandom = "WR";
		const string stringCommand_WaitForTesterInputs = "WO";
		const string stringCommand_WaitOutput_Camera = "WC";
		const string stringCommand_Wait = "W";
		const string stringCommand_WaitImage = "WFII";
		const string stringCommand_SubScript = "SUBSCPT";
		const string stringCommand_WaitUser = "WFUI";
		const string stringCommand_DebugWaitUser = "DWFUI";
		const string stringCommand_FlashRenesas = "FR";
		const string stringCommand_FlashMicrochip = "FM";
		const string stringCommand_FlashCypress = "FC";
		const string stringCommand_FlashInventek = "FI";
		const string stringCommand_EraseRenesas = "ER";
		const string stringCommand_EraseCypress = "EC";
		const string stringCommand_ReadUart = "RUART";
		const string stringCommand_SearchUart = "SUART";
		const string stringCommand_TransmitUart = "TUART";
		const string stringCommand_RepeatStart = "RPT_START";
		const string stringCommand_RepeatEnd = "RPT_END";

		const string stringIoDirection_Input = "I";
		const string stringIoDirection_Output = "O";

		string stringBoardOnTester = "BoardOnTester";	// attribute
		
		private XElement SearchBoardByPosition(string position)
		{
			if (testXmlDoc != null)
			{
				if (position.Length > 0)
				{
					var info = from element in testXmlDoc.Descendants(stringBoardOnTester)
							   where ((string)element.Element(stringBoardIndexOnTester) == position)
							   select element;
					if (info.Count() > 0) return (XElement)info.Distinct().First();
				}
			}
			return null;
		}

		private XElement SearchModuleByID(string id)
		{
			if (modulesXmlDoc != null)
			{
				if (id.Length > 0)
				{
					var info = from element in modulesXmlDoc.Descendants(stringModule)
							   where ((string)element.Element(stringModuleCodeName) == id)
							   select element;
					if (info.Count() > 0) return (XElement)info.Distinct().First();
				}
			}
			return null;
		}

		private XElement SearchModuleByPosition(string position)
		{
			if (testXmlDoc != null)
			{
				if (position.Length > 0)
				{
					XElement board = SearchBoardByPosition(position);
					if (board != null)
					{
						string id = (string)board.Element(stringModuleCodeName);
						if (id != null && id.Length > 0)
						{
							return SearchModuleByID(id);
						}
					}
				}
			}
			return null;
		}

		private XElement SearchForProbeElement(string alias, string board, string pin)
		{
			foreach (XElement obj in testXmlDoc.Descendants(stringConnection))
			{
				string ioName = (string)obj.Element(stringConnectionAlias);
				string ioBoard = (string)obj.Element(stringBoardIndexOnTester);
				string ioPinNumber = (string)obj.Element(stringPinNumberOnBoard);
				if (ioName != null && (alias.Length == 0 || ioName == alias) &&
					ioBoard != null && (board.Length == 0 || ioBoard == board) &&
					ioPinNumber != null && (pin.Length == 0 || ioPinNumber == pin))
				{
					return obj;
				}
			}
			return null;
		}

		private XElement SearchForProbeElementWithinRange(string alias, string board, string pin, int searchSize)
		{
			for (int i = 0; i < testXmlDoc.Descendants(stringConnection).Count() && (searchSize == 0 || i < searchSize); i++)
			{
				XElement obj = testXmlDoc.Descendants(stringConnection).ElementAt(i);
				string ioName = (string)obj.Element(stringConnectionAlias);
				string ioBoard = (string)obj.Element(stringBoardIndexOnTester);
				string ioPinNumber = (string)obj.Element(stringPinNumberOnBoard);
				if (ioName != null && (alias.Length == 0 || ioName == alias) &&
					ioBoard != null && (board.Length == 0 || ioBoard == board) &&
					ioPinNumber != null && (pin.Length == 0 || ioPinNumber == pin))
				{
					return obj;
				}
			}
			return null;
		}

		private XElement SearchModuleByProbeName(string objName)
		{
			foreach (XElement obj in testXmlDoc.Descendants(stringConnection))
			{
				string ioName = (string)obj.Element(stringConnectionAlias);
				string ioBoard = (string)obj.Element(stringBoardIndexOnTester);
				if (ioName != null && ioName == objName &&
					ioBoard != null && ioBoard.Length > 0)
				{
					XElement board = SearchBoardByPosition(ioBoard);
					if (board != null)
					{
						string ioModuleId = (string)board.Element(stringModuleCodeName);
						if (ioModuleId != null && ioModuleId.Length > 0)
						{
							XElement module = SearchModuleById(ioModuleId);
							return module;
						}
					}
				}
			}
			return null;
		}

		private XElement SearchModuleById(string id)
		{
			if (modulesXmlDoc != null)
			{
				if (id.Length > 0)
				{
					var info = from element in modulesXmlDoc.Descendants(stringModule)
							   where ((string)element.Element(stringModuleCodeName) == id)
							   select element;
					if (info.Count() > 0) return (XElement)info.Distinct().First();
				}
			}
			return null;
		}

		private string SearchCommandAliasByCodeName(string commandCodeName)
		{
			XElement e = SearchCommandByCodeName(commandCodeName);
			if (e != null) return e.Element(stringCommandAlias).Value;
			return "";
		}
		
		private string SearchParameterAliasByCodeName(string commandCodeName, string parameterCodeName, string parameterField)
		{
			XElement e = SearchParameterByCodeName(commandCodeName, parameterCodeName);
			if (e != null) return e.Element(parameterField).Value;
			return "";
		}

		private XElement SearchParameterByCodeName(string commandCodeName, string parameterId)
		{
			if (modulesXmlDoc != null)
			{
				if (parameterId.Length > 0)
				{
					XElement command = SearchCommandByCodeName(commandCodeName);
					var info = from element in command.Descendants(stringParameter)
							   where ((string)element.Element(stringParameterCodeName) == parameterId)
							   select element;
					if (info.Count() > 0) return (XElement)info.Distinct().First();
				}
			}
			return null;
		}

		const string stringParameter = "Parameter";	// attribute
		const string stringParameterCodeName = "ParameterCodeName";	// element
		const string stringParameterAlias = "ParameterAlias";	// element
		const string stringParameterValueMin = "ValueMin";	// element
		const string stringParameterValueMax = "ValueMax";	// element

		private string SearchCommandCodeNameByAlias(string probeName, string commandAlias)
		{
			XElement e = SearchCommandByAlias(probeName,commandAlias);
			if (e != null) return e.Element(stringCommandCodeName).Value;
			return "";
		}

		private XElement SearchCommandByAlias(string probeName, string commandAlias)
		{
			if (modulesXmlDoc != null && commandAlias.Length > 0)
			{
				if (probeName.Length > 0)
				{
					XElement pin = SearchPinByProbeName(probeName);
					if (pin != null)
					{
						foreach (XElement cmd in pin.Descendants(stringApplicableCommand))
						{
							string commandId = (string)cmd.Element(stringCommandCodeName);
							if (commandId != null && commandId.Length > 0)
							{
								XElement command = SearchCommandByCodeName(commandId);
								if (command != null)
								{
									string commandName = (string)command.Element(stringCommandAlias);
									if (commandName != null && commandName == commandAlias)
									{
										return command;
									}
								}
							}
						}
					}
				}
				else
				{
					var info = from element in modulesXmlDoc.Descendants(stringCommand)
							   where ((string)element.Element(stringCommandAlias) == commandAlias)
							   select element;
					if (info.Count() > 0) return (XElement)info.Distinct().First();
				}
			}
			return null;
		}
		
		private XElement SearchCommandByCodeName(string commandCodeName)
		{
			if (modulesXmlDoc != null)
			{
				if (commandCodeName.Length > 0)
				{
					var info = from element in modulesXmlDoc.Descendants(stringCommand)
							   where ((string)element.Element(stringCommandCodeName) == commandCodeName)
							   select element;
					if (info.Count() > 0) return (XElement)info.Distinct().First();
				}
			}
			return null;
		}

		private XElement SearchPinById(string modulePosition, string pinNumber)
		{
			if (modulesXmlDoc != null)
			{
				if (pinNumber.Length > 0)
				{
					XElement module = SearchModuleByPosition(modulePosition);
					var info = from element in module.Descendants(stringPin)
							   where ((string)element.Attribute(stringId) == pinNumber)
							   select element;
					if (info.Count() > 0) return (XElement)info.Distinct().First();
				}
			}
			return null;
		}

		private XElement SearchPinByProbeName(string probeName)
		{
			if (modulesXmlDoc != null)
			{
				foreach (XElement obj in testXmlDoc.Descendants(stringConnection))
				{
					string ioName = (string)obj.Element(stringConnectionAlias);
					string pinNumber = (string)obj.Element(stringPinNumberOnBoard);
					if (ioName != null && ioName == probeName && pinNumber != null && pinNumber.Length > 0)
					{
						XElement module = SearchModuleByProbeName(probeName);
						var info = from element in module.Descendants(stringPin)
								   where ((string)element.Attribute(stringId) == pinNumber)
								   select element;
						if (info.Count() > 0) return (XElement)info.Distinct().First();
					}
				}
			}
			return null;
		}

		private void ShowTesterConnections(string boardIndex, string channelIndex)
		{
			TreeNode toBeSelectedNode = null;// new TreeNode();
			treeViewBoards.Nodes.Clear();
			excelReportBoards.Clear();
			treeViewConnections.Nodes.Clear();
			excelReportConnections.Clear();
			if (testXmlDoc != null)
			{
				for (int i = 1; ; i++)
				{
					string position = i.ToString();
					XElement module = SearchModuleByPosition(position);
					if (module != null)
					{
						string name = module.Element(stringModuleAlias).Value;
						if (name != null && name.Length > 0)
						{
							string str = GetBoardName(name, position);
							TreeNode node = treeViewConnections.Nodes.Add(str);
							excelReportConnections.Add(str);
							node.Tag = position;
							TreeNode node1 = treeViewBoards.Nodes.Add(str);
							node1.Tag = position;
							excelReportBoards.Add(str);
							int j = 0;
							foreach (XElement pin in module.Descendants(stringPin))
							{
								string pinAlias = (string)pin.Element(stringPinAlias);
								string ioDirection = (string)pin.Element(stringIoDirection).Value;
								j++;
								str = "P." + j.ToString() + "   ";
								if (ioDirection == "-") str += "-" + pinAlias + "-";
								else str += pinAlias;
								TreeNode n = node.Nodes.Add(str);
								TreeNode n1 = node1.Nodes.Add(str);
								XElement probe = SearchForProbeElement("", position, j.ToString());
								n.Tag = j.ToString();
								string ioName = "";
								if (probe != null)
								{
									ioName = (string)probe.Element(stringConnectionAlias);
									if (ioName == null) ioName = "";
								}
								if (ioName.Length > 0)
								{
									switch (ioDirection)
									{
										case stringIoDirection_Input:	// inpout
											ioName = "  <---------------  " + ioName;
											break;
										case stringIoDirection_Output:	// output
											ioName = "  --------------->  " + ioName;
											break;
										default:
											ioName = "  <--------------->  " + ioName;
											break;
									}
								}
								n.Text += ioName;
								excelReportConnections.Add("\t" + n.Text);
								excelReportBoards.Add("\t" + n1.Text);
								if (position == boardIndex && j.ToString() == channelIndex)
								{
									toBeSelectedNode = n;
								}
							}
							node.Expand();
						}
					}
					else
					{
						break;
					}
				}
			}
			treeViewConnections.SelectedNode = toBeSelectedNode;
		}

		private void buttonAddAbove_Click(object sender, EventArgs e)
		{
			InsertTestStep(false);
		}

		private void InsertTestStep(bool AtBelow)
		{
			if (listViewTestSteps.Items.Count == 0)
			{
				AddTestStep(1);
				LoadTestFile(1);
			}
			else
			{
				int id = GetSelectedTest();
				if (id >= 1)
				{
					if (AtBelow) id++;
					if (MoveDownSteps(id))
					{
						AddTestStep(id);
						LoadTestFile(id);
					}
				}
			}
		}

		private bool AddTestStep(int id)
		{		
			XElement newElement =
					new XElement(stringStep, new XAttribute(stringId, id.ToString("D4")),
						new XElement(stringCommandCodeName, ""));
			testXmlDoc.Descendants(stringSetup).First().Add(newElement);
			return true;
		}

		private bool AdjustRealValuePerPiValue(XElement obj, string realValueString, string piValueString)
		{
			string value = obj.Element(realValueString).Value;
			string _value = obj.Element(piValueString).Value;
			if (realValueString == stringRepeatNumber)
			{
				if (value != _value)
				{
					obj.Element(realValueString).Value = _value;
					return true;
				}
			}
			else
			{
				if (value.Length > 0)
				{
					try
					{
						int msValue = 0;
						if (realValueString == stringValue) msValue = ConvertTextToNumber(value, false, true, true, false);
						else msValue = ConvertTextToNumber(value, false, false, true, false);

						string str = msValue.ToString();
						if (str != _value)
						{
							double f = Convert.ToDouble(_value) / 1000;
							obj.Element(realValueString).Value = f.ToString();
							return true;
						}
					}
					catch
					{
					}
				}
			}
			return false;
		}

		private int CorrectManualEditErrors()
		{
			int bChangedMade = 0;
			if (testXmlDoc != null)
			{
				int currentStepId = 0;
				int expectedStepId = 0;
				bool toShowMessage = true;
				bool bCorrectId = false;
				foreach (XElement obj in testXmlDoc.Descendants(stringStep))
				{
					if (obj.Element(stringCommandCodeName) == null) continue;

					// check step ID
					expectedStepId++;
					if (bCorrectId == false)
					{
						currentStepId = Convert.ToInt32(obj.Attribute(stringId).Value);
						if (currentStepId != expectedStepId)
						{
//							MessageBox.Show(expectedStepId.ToString() + " --> " + currentStepId.ToString());
							bChangedMade++;
							bCorrectId = true;
						}
					}
					if (bCorrectId)
					{
						obj.Attribute(stringId).Value = expectedStepId.ToString("D4");
					}
					if (obj.Element(stringCommandName) == null) continue;

					// check command code
					string command = obj.Element(stringCommandCodeName).Value;
					string alias = obj.Element(stringCommandName).Value;
					if (command.Length == 0 || alias.Length == 0 || alias != GetCommandAlias(command))
					{
						foreach (XElement cmd in modulesXmlDoc.Descendants(stringCommand))
						{
							string commandCode = cmd.Element(stringCommandCodeName).Value;
							if (GetCommandAlias(commandCode) == alias)
							{
								obj.Element(stringCommandCodeName).Value = commandCode;
								MessageBox.Show(command + " -> " + commandCode + " for " + "\"" + alias + "\" -- Step # " + currentStepId.ToString());
								bChangedMade++;
							}
						}
					}
					// update the outdated commands
					if (obj.Element(stringCommandName).Value == GetCommandAlias(stringCommand_WaitOutput_Camera) &&
						obj.Element(stringCommandCodeName).Value == stringCommand_WaitForTesterInputs)
					{
						obj.Element(stringCommandCodeName).Value = stringCommand_WaitOutput_Camera;
//						MessageBox.Show("Manually added \"Wait for Camera\" command was found and automatically updated -- Step " + obj.Attribute(stringId));
						bChangedMade++;
					}
					// some manually added values are incorrect 
					bool error1 = false;
					bool error2 = false;
					switch (obj.Element(stringCommandCodeName).Value)
					{
						case stringCommand_WaitRandom:
						case stringCommand_WaitForTesterInputs:
						case stringCommand_WaitOutput_Camera:
							error1 = AdjustRealValuePerPiValue(obj, stringMinValue, stringMinValue_ForPi);
							error2 = AdjustRealValuePerPiValue(obj, stringMaxValue, stringMaxValue_ForPi);
							break;
						case stringCommand_Wait:
							error1 = AdjustRealValuePerPiValue(obj, stringValue, stringValue_ForPi);
							break;
						case stringCommand_RepeatStart:
							error1 = AdjustRealValuePerPiValue(obj, stringRepeatNumber, stringRepeatNumber_ForPi);
							break;
					}
					if (error1 || error2)
					{
//						MessageBox.Show("Incorrectly manually added script was found and has been automatically corrected -- Step " + obj.Attribute(stringId));
						bChangedMade++;
					}
					//WFUIII correct to WFII
					/*//////////////////////////////////////////
					check added _CH that are not defined as a stringConnection
					check added _CH that are not written as a "Probe"
    <Probe ID="xx">
      <Name> name </Name>
      <BoardId> known </BoardId>
      <ChannelId> CC </ChannelId>
      <Value>???</Value>
    </Probe>
    <_CH CC>???</_CH CC>*/
	/*	const string stringMinValue = "MinValue";
		const string stringMaxValue = "MaxValue";
		const string stringMinValue_ForPi = "_MinValue";
		const string stringMaxValue_ForPi = "_MaxValue";
*/

					// some manually added pins are not defined as a probe
					string ManualyAddedPowerBoardChannel = "5";
					if (obj.Descendants("_Board").Count() > 0 &&
						obj.Element("_Board").Value == "PowerBoard" &&
						obj.Descendants("_CH" + ManualyAddedPowerBoardChannel).Count() > 0)
					{
						string boardPosition = (string)obj.Element("BoardId").Value;
						// change board name 
						XElement board = SearchBoardByPosition(boardPosition);
						if (board.Element(stringModuleCodeName).Value == "P")
						{
							if (toShowMessage)
							{
								toShowMessage = false;
								MessageBox.Show("Incorrectly manually added script for the old power board is found, and has been automatically corrected. -- Step " + obj.Attribute(stringId));
							}
							// change to the new power board
							board.Element(stringModuleCodeName).Value = "P5";
							// Remove connections at Pin# 3, 4
							XElement probe3 = SearchForProbeElement("", boardPosition, "3");
							if (probe3 != null) probe3.Remove();
							XElement probe4 = SearchForProbeElement("", boardPosition, "4");
							if (probe4 != null) probe4.Remove();
							// add probe #5
							var info = from element in obj.Descendants("Probe")
									   select element;
							if (info != null && info.Count() > 0)
							{
								foreach (XElement element in info.Distinct())
								{
									string probeName = element.Element("Name").Value;
									string value = element.Element("ChannelId").Value;
									if (probeName.Length > 0 && value == ManualyAddedPowerBoardChannel)
									{
										AddProbe(probeName, boardPosition, ManualyAddedPowerBoardChannel);
										break;
									}
								}
							}
							bChangedMade++;
						}
					}
				}
			}
			return bChangedMade;
		}

		private int ReArrangeProbeOrder()
		{
			int bChangedMade = 0;
			if (testXmlDoc != null)
			{
				for (int i = 1; i < testXmlDoc.Descendants(stringConnection).Count(); i++)
				//foreach (XElement obj in testXmlDoc.Descendants(stringConnection))
				{
					XElement obj = testXmlDoc.Descendants(stringConnection).ElementAt(i);
					if (FindRightPlaceForProbe(obj, i)) bChangedMade = 1;
				}
			}
			return bChangedMade;
		}

		private bool AddProbe(string probeAlias, string boardIndexOnTester, string pinNumberOnBoard)
		{
			XElement obj = SearchForProbeElement("", boardIndexOnTester, pinNumberOnBoard);
			if (obj == null)
			{
				string id = FindNextProbeId();
				XElement newElement =
						new XElement(stringConnection, new XAttribute(stringId, id),
							new XElement(stringConnectionAlias, probeAlias),
							new XElement(stringBoardIndexOnTester, boardIndexOnTester),
							new XElement(stringPinNumberOnBoard, pinNumberOnBoard)
						);
				testXmlDoc.Descendants(stringSetup).First().Add(newElement);
				FindRightPlaceForProbe(newElement, 0);
			}
			return true;
			/*
			bool FoundProperLocationForPin = false;
			for (int i = Convert.ToInt32((string)pinNumberOnBoard) + 1; i < 100; i++)
			{
				XElement probe = SearchForProbeElement("", boardIndexOnTester, i.ToString());
				if (probe != null)
				{
					//testXmlDoc.Descendants(stringSetup).First().AddBeforeSelf
					probe.AddBeforeSelf(newElement);
					FoundProperLocationForPin = true;
					break;
				}
			}
			if (FoundProperLocationForPin == false)
			{
				for (int i = Convert.ToInt32((string)pinNumberOnBoard) - 1; i > 0; i--)
				{
					XElement probe = SearchForProbeElement("", boardIndexOnTester, i.ToString());
					if (probe != null)
					{
						probe.AddAfterSelf(newElement);
						FoundProperLocationForPin = true;
						break;
					}
				}
			}
			if (FoundProperLocationForPin == false)
			{
				testXmlDoc.Descendants(stringSetup).First().Add(newElement);
			}
			return true;
			 * */
		}

		private bool FindRightPlaceForProbe(XElement element, int searchSize)
		{
			string boardIndexOnTester = (string)element.Element(stringBoardIndexOnTester);
			string pinNumberOnBoard = (string)element.Element(stringPinNumberOnBoard);

			for (int i = Convert.ToInt32((string)pinNumberOnBoard) + 1; i < 100; i++)
			{
				XElement probe = SearchForProbeElementWithinRange("", boardIndexOnTester, i.ToString(), searchSize);
				if (probe != null)
				{
					if (probe.PreviousNode != element)
					{
						element.Remove();
						probe.AddBeforeSelf(element);
						return true;
					}
					else
					{
						return false;
					}
				}
			}
			for (int i = Convert.ToInt32((string)pinNumberOnBoard) - 1; i > 0; i--)
			{
				XElement probe = SearchForProbeElementWithinRange("", boardIndexOnTester, i.ToString(), searchSize);
				if (probe != null)
				{
					if (element.PreviousNode != probe)
					{
						element.Remove();
						probe.AddAfterSelf(element);
						return true;
					}
					else
					{
						return false;
					}
				}
			}
			return false;
		}

		private bool RemoveTestStep(int id)
		{
			XElement test = SearchElementById(stringStep, id);
			if (test != null)
			{
				test.Remove();
				return true;
			}
			return false;
		}

		private void SwapTestStep(bool MoveUp)
		{
			int id = GetSelectedTest();
			if (id >= 1)
			{
				int newId = id;
				if (MoveUp) newId--;
				else newId++;
				XElement step0 = SearchElementById(stringStep, id);
				XElement step1 = SearchElementById(stringStep, newId);
				if (step0 != null && step1 != null)
				{
					step0.Attribute(stringId).Value = newId.ToString("D4");
					step1.Attribute(stringId).Value = id.ToString("D4");
					LoadTestFile(newId);
				}
			}
		}

		private int GetSelectedTest()
		{
			if (listViewTestSteps.SelectedIndices.Count == 1)
			{
				return listViewTestSteps.SelectedIndices[0] + 1;	// +1 to compensate the index being started at 0
			}
			return 0;	// nothing selected
		}

		private void buttonRemove_Click(object sender, EventArgs e)
		{
			int id = GetSelectedTest();
			if (id >= 1)
			{
				if (RemoveTestStep(id))
				{
					MoveUpSteps(id);
					if (SearchElementById(stringStep, 1) == null) AddTestStep(1);
					LoadTestFile(id);
				}
			}
		}

		private void buttonAddBelow_Click(object sender, EventArgs e)
		{
			InsertTestStep(true);
		}

		private void buttonMoveUp_Click(object sender, EventArgs e)
		{
			SwapTestStep(true);
		}

		private string GetParentFolder(string path, int excludeFolder)
		{
			if (path.Length > 3)
			{
				if (path[path.Length - 1] == '\\') path = path.Substring(0, path.Length - 1);
				int index = path.LastIndexOf('\\'); // exclude the last "\"
				if (index > 0)
				{
					if (excludeFolder == 0) return path.Substring(0, index + 1);
				}
			}
			return "";
		}

		string currentFile = "";
		string stringNewTestFile = "";
		string stringTesterConfigFile = "";
		string stringModulesFile = "";

		string constStringFolderFiles = "Files\\";
		string constStringModulesFile = "modules.xml";
		string constStringNewTestFile = "new.xml";
		string constStringTesterConfigFile = "testerconfig.xml";

		const string stringCommand = "Command";
		const string stringSetup = "Setup";
		const string stringStep = "Step";
		const string stringConnection = "Connection";
		const string stringConnectionAlias = "ConnectionAlias";
		const string stringBoardIndexOnTester = "BoardIndexOnTester";
		const string stringPinNumberOnBoard = "ChannelIndexOnBoard";
		const string stringCommandName = "_Command";
		const string stringTestStepNote = "TestStepNote";
		const string stringStepParameterIndex = "ParameterCodeName-";
		const string stringStepParaValue = "ParameterValue-";
		const string stringScript = "Script";
		const string stringId = "ID";
		const string stringBarCode = "Barcode";
		const string stringDatalog = "Datalog";
		const string stringNoStopAtFailure = "NoStopAtFailure";
		const string stringScriptAutoCorrectVersion = "AutoCorrectVersion";
		const string stringTestStepDisabled = "Disabled";


		const string stringValue = "Value";
		const string stringValue_ForPi = "_Value";
		const string stringMinValue = "MinValue";
		const string stringMaxValue = "MaxValue";
		const string stringMinValue_ForPi = "_MinValue";
		const string stringMaxValue_ForPi = "_MaxValue";
		const string stringUnit_ForPi = "_Unit";
		const string stringRepeatNumber = "RepeatNumber";
		const string stringRepeatNumber_ForPi = "_RepeatNumber";

		const int MAX_PARAMETERS = 5;

		XDocument testXmlDoc = new XDocument();
		XDocument modulesXmlDoc = new XDocument();
		//XDocument testerConfigXmlDoc = new XDocument();


		private bool FindOrReplaceTestConnectionAlias(string alias, string replacedByNewAlias)
		{
			if (testXmlDoc != null)
			{
				if (alias.Length > 0)
				{
					var info = from element in testXmlDoc.Descendants(stringStep)
							   where ((string)element.Element(stringConnectionAlias) == alias)
							   select element;
					if (info.Count() > 0)
					{
						if (replacedByNewAlias.Length > 0)
						{
							foreach (XElement element in info.Distinct())
							{
								element.Element(stringConnectionAlias).Value = replacedByNewAlias;
							}
						}
						return true;
					}
				}
			}
			return false;
		}

		private XElement SearchElementById(string elementString, int elementId)
		{
			if (testXmlDoc != null)
			{
				if (elementId > 0)
				{
					var info = from element in testXmlDoc.Descendants(elementString)
							   where (Convert.ToInt32((string)element.Attribute(stringId)) == elementId)
							   select element;
					if (info.Count() > 0) return (XElement)info.Distinct().First();
				}
			}
			return null;
		}

		private XElement SearchElementByTabName(string elementString)
		{
			if (testXmlDoc != null)
			{
				var info = from element in testXmlDoc.Descendants(elementString)
						   select element;
					if (info.Count() > 0) return (XElement)info.Distinct().First();
			}
			return null;
		}

		private bool MoveDownSteps(int id)
		{
			if (testXmlDoc != null)
			{
				foreach (XElement step in testXmlDoc.Descendants(stringStep))
				{
					string str = (string)step.Attribute(stringId);
					if (str != null && str.Length > 0)
					{
						int stepId = Convert.ToInt32(str);
						if (stepId >= id)
						{
							step.Attribute(stringId).Value = (stepId + 1).ToString("D4");
						}
					}
				}
				return true;
			}
			return false;
		}

		private bool MoveUpSteps(int id)
		{
			if (testXmlDoc != null)
			{
				foreach (XElement step in testXmlDoc.Descendants(stringStep))
				{
					string str = (string)step.Attribute(stringId);
					if (str != null && str.Length > 0)
					{
						int stepId = Convert.ToInt32(str);
						if (stepId > id)
						{
							step.Attribute(stringId).Value = (stepId - 1).ToString("D4");
						}
					}
				}
				return true;
			}
			return false;
		}

		private void LoadTestFile(int id)
		{
			ShowTestSteps();
			if (id >= 1)
			{
				id--;	// -1 to compensate the index being started at 0
				listViewTestSteps.Focus();
				if (listViewTestSteps.Items.Count > id)
				{
					listViewTestSteps.Items[id].Selected = true;
					listViewTestSteps.EnsureVisible(id);
				}
			}
		}

		private string FindNextProbeId()
		{
			if (testXmlDoc != null)
			{
				for (int i = 1; ; i++)
				{
					XElement test = SearchElementById(stringConnection, i);
					if (test == null) return i.ToString();
				}
			}
			return "";
		}

		private void ShowTestSettings()
		{
			XElement barcode = SearchElementByTabName(stringBarCode);
			if (barcode != null) checkBoxBarcode.Checked = true;
			else checkBoxBarcode.Checked = false;
			XElement datalog = SearchElementByTabName(stringDatalog);
			if (datalog != null) checkBoxDataLoggerMode.Checked = true;
			else checkBoxDataLoggerMode.Checked = false;
			XElement noStopAtFailure = SearchElementByTabName(stringNoStopAtFailure);
			if (noStopAtFailure != null) checkBoxNoStop.Checked = true;
			else checkBoxNoStop.Checked = false;
		}

		private void ShowTestSteps()
		{
			listViewTestSteps.Items.Clear();
			excelReportTestSteps.Clear();
			if (testXmlDoc != null)
			{
				for (int i = 1; ; i++)
				{
					XElement test = SearchElementById(stringStep, i);
					if (test != null)
					{
						string id = i.ToString("D4");
						string testName = (string)test.Element(stringTestStepNote);
						if (testName == null) testName = "";
						string command = (string)test.Element(stringCommandCodeName);
						if (command == null) command = "";
						string cmdAlias = SearchCommandAliasByCodeName(command);
						bool disabled = false;
						if (test.Element(stringTestStepDisabled) != null) disabled = true;
#if old_scheme
						string connectionAlias = (string)test.Element(stringConnectionAlias);
						if (connectionAlias == null) connectionAlias = "";
						string parameters = "";
						for (int para = 0; para < MAX_PARAMETERS; para++)
						{
							string parameterIndex = stringStepParameterIndex + para.ToString();
							string parameterValue = stringStepParaValue + para.ToString();
							string codeName = (string)test.Element(parameterIndex);
							string value = (string)test.Element(parameterValue);
							if (codeName != null && codeName.Length > 0 && value != null && value.Length > 0)
							{
								/*
								string parameterAlias = SearchParameterAliasByCodeName(pin, codeName, stringParameterAlias);
								parameters += parameterAlias + "=" + value + " ";
								 * */
								if (para == 0) parameters += value;
								else parameters += " / " + value;
							}
						}
						ListViewItem item = new ListViewItem(new[] { id, connectionAlias, cmdAlias, parameters, testName });
						listViewTestSteps.Items.Add(item);
						string str = "'"+id + "\t" + connectionAlias + "\t" + cmdAlias + "\t" + parameters + "\t" + testName;
						excelReportTestSteps.Add(str);
#else
						string probeState = "";
						switch (command)
						{
							case stringCommand_SetTesterOutput:
							case stringCommand_BarcodeScan:
							case stringCommand_BarcodeScan_Old:
							case stringCommand_VerifyTesterInput:
							case stringCommand_WaitForTesterInputs:
							case stringCommand_WaitOutput_Camera:
								var info = from element in test.Descendants("Probe")
										   select element;
								if (info != null && info.Count() > 0)
								{
									foreach (XElement element in info.Distinct())
									{
										string name = element.Element("Name").Value;
										string value = element.Element(stringValue).Value;
										if (name.Length > 0 && value.Length > 0)
										{
											if (probeState.Length > 0) probeState += "; ";
											probeState += "[" + name + "=] " + value;
										}
									}
								}
								break;
						}
						string timeParameter = "";
						switch (command)
						{
							case stringCommand_WaitRandom:
							case stringCommand_WaitForTesterInputs:
							case stringCommand_WaitOutput_Camera:
								XElement min = test.Element(stringMinValue);
								XElement max = test.Element(stringMaxValue);
								if (min != null && min.Value.Length > 0) timeParameter += min.Value;
								else timeParameter += "0";
								timeParameter += " to ";
								if (max != null && max.Value.Length > 0) timeParameter += max.Value;
								else timeParameter += "-";
								timeParameter += " (sec)";
								break;
							case stringCommand_Wait:
								XElement wait = test.Element(stringValue);
								if (wait != null) timeParameter += wait.Value;
								else timeParameter += "0";
								timeParameter += " (sec)";
								break;
							case stringCommand_RepeatStart:
								XElement repeat = test.Element(stringRepeatNumber);
								if (repeat != null)
								{
									probeState = "[Repeat=] ";
									if (repeat.Value.Length == 0)
									{
										probeState = "Prompt operator";
									}
									else if (repeat.Value == "0")
									{
										probeState += "None";
									}
									else
									{
										probeState += repeat.Value;
										probeState += " (Times)";
									}
								}
								break;
							case stringCommand_WaitUser:
							case stringCommand_DebugWaitUser:
							case stringCommand_RepeatEnd:
							case stringCommand_EraseRenesas:
							case stringCommand_EraseCypress:
								break;
							case stringCommand_FlashRenesas:
							case stringCommand_FlashMicrochip:
							case stringCommand_FlashCypress:
								XElement hexFile = test.Element("FirmwareFile");
								if (hexFile != null) probeState = "[FirmwareFile=] " + hexFile.Value;
								break;
							case stringCommand_FlashInventek:
								XElement macFile = test.Element("MacLogFile");
								if (macFile != null) probeState = "[MacLogFile=] " + macFile.Value;
								break;
							case stringCommand_ReadUart:
							case stringCommand_SearchUart:
							case stringCommand_TransmitUart:
								XElement stringId = test.Element("MessageString");
								if (stringId != null) probeState = "[Message=] " + stringId.Value;
								break;
							case stringCommand_SubScript:
								XElement subscript = test.Element("SubScript");
								if (subscript != null) probeState = "[Subscript=] " + subscript.Value;
								break;
							case stringCommand_WaitImage:
								XElement image = test.Element("Image");
								if (image != null) probeState = "[Image=] " + image.Value;
								break;
						}
						if (disabled) id += " - Disabled";
						//if (command == stringCommand_DebugWaitUser) id += " <Debug>";
						ListViewItem item = new ListViewItem(new[] { id, cmdAlias, timeParameter, probeState, testName });
						listViewTestSteps.Items.Add(item);
						string str = "'" + id + "\t" + cmdAlias + "\t" + timeParameter + "\t" + probeState + "\t" + testName;
						excelReportTestSteps.Add(str);
#endif
					}
					else
					{
						break;
					}
				}
			}
		}

		private void buttonMoveDown_Click(object sender, EventArgs e)
		{
			SwapTestStep(false);
		}

		private void comboBoxTestCommand_DropDownClosed(object sender, EventArgs e)
		{
#if old_scheme
			XElement cmd = SearchCommandByAlias(comboBoxTestObject.Text, comboBoxTestCommand.Text);
			if (cmd != null && comboBoxTestCommand.Text.Length > 0)
			{
				dataGridViewParameters.Rows.Clear();
				foreach (XElement parameter in cmd.Descendants(stringParameter))
				{
					string alias = (string)parameter.Element(stringParameterAlias);
					string codename = (string)parameter.Element(stringParameterCodeName);
					string min = (string)parameter.Element(stringParameterValueMin);
					string max = (string)parameter.Element(stringParameterValueMax);
					if (alias != null && alias.Length > 0)
					{
						DataGridViewRow r = new DataGridViewRow();
						r.CreateCells(dataGridViewParameters);
						r.Cells[0].Value = alias;
						r.Cells[1].Value = "                  ";
						r.Cells[2].Value = codename;
						r.Cells[3].Value = min;
						r.Cells[4].Value = max;
						//r.SetValues(cmdAlias, "", codename, min, max);
						dataGridViewParameters.Rows.Add(r);
					}
				}
			}
#endif
		}

		private void comboBoxTestObject_DropDownClosed(object sender, EventArgs e)
		{
#if old_scheme
			//comboBoxTestObject.Text = "";
#endif
		}

		private int ConvertTextToNumber(string str, bool mustBe1Or0, bool cannotBeEmpty, bool convertToMilliseconds, bool allowToBe0)
		{
			try
			{
				int floatingPointPosition = -1;
				if (str.Length > 0)
				{
					if (mustBe1Or0)
					{
						if (str != "1" && str != "0") return -1;
					}
					else
					{
						for (int i = 0; i < str.Length; i++)
						{
							if (Char.IsDigit(str[i]) == false)
							{
								if (str[i] == '.' && i > 0 && i < str.Length - 1 && floatingPointPosition == -1)	// allow '.' to appear once at the middle
								{
									floatingPointPosition = i;
								}
								else
								{
									return -1;
								}
							}
							else
							{
								if (str[0] == '0' && allowToBe0 == false && i >= 1 && str[1] != '.') return -1;	// if first digit is '0', second must be '.'
							}
						}
					}
				}
				else
				{
					if (cannotBeEmpty)
					{
						return -1;
					}
					else
					{
						return -100;
					}
				}
				if (convertToMilliseconds)
				{
					if (floatingPointPosition == -1)
					{
						if (str.Length > 0)
						{
							int ms = Convert.ToInt32(str + "000");	// equivalent to multipled by 1000 - converting from seconds to milliseconds
							return ms;
						}
						else return 0;
					}
					else if (floatingPointPosition > 0 && floatingPointPosition < str.Length - 1)
					{
						string dec = str.Substring(0, floatingPointPosition);
						string floating = str.Substring(floatingPointPosition + 1);
						switch (floating.Length)
						{
							case 0: return -1;
							case 1: floating += "00"; break;
							case 2: floating += "0"; break;
							case 3: break;
							default: floating = floating.Substring(0, 3); break;
						}
						if (dec.Length > 0 && floating.Length > 0)
						{
							int ms = Convert.ToInt32(dec + "000");	// equivalent to multipled by 1000 - converting from seconds to milliseconds
							return ms + Convert.ToInt32(floating);
						}
					}
				}
				else
				{
					if (floatingPointPosition == -1 && str.Length > 0) return Convert.ToInt32(str);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			return -1;
		}

		private void buttonTestUpdate_Click(object sender, EventArgs e)
		{
			int id = GetSelectedTest();
			if (id >= 1)
			{
				UpdateTestStep(id, false);
			}
//			listViewTestSteps.SelectedIndices.Clear();
//			listViewTestSteps.SelectedIndices.Add(id - 1);	// +1 to compensate the index being started at 0
			return;
		//	if (listViewTestSteps.SelectedIndices.Count > 3)
			{
				listViewTestSteps.SelectedIndices.Clear();
				listViewTestSteps.SelectedIndices.Add(3);	// +1 to compensate the index being started at 0
			}
/*
			if (testXmlDoc != null)
			{
				foreach (XElement step in testXmlDoc.Descendants(stringStep))
				{
					string str = (string)step.Attribute(stringId);
					if (str != null && str.Length > 0)
					{
						int stepId = Convert.ToInt32(str);
						PopulateCommandDetails();
*/						listViewTestSteps_SelectedIndexChanged(sender, e);
						id = GetSelectedTest();
						if (id >= 1)
						{
							UpdateTestStep(id, false);
						}
/*					}
				}
			}		
*/		}

		private void UpdateTestStep(int id, bool ToDisable)
		{
			XElement test = SearchElementById(stringStep, id);
			if (test != null)
			{
				string command = GetCurrentSelectedCommand();
				if (command.Length == 0) return;
#if	old_scheme
				test.Element(stringCommandCodeName).Value = SearchCommandCodeNameByAlias(comboBoxTestObject.Text, comboBoxTestCommand.Text);
				test.Element(stringConnectionAlias).Value = comboBoxTestObject.Text;
				// save parameters
				for (int para = 0; para < MAX_PARAMETERS; para++)
				{
					string parameterType = stringStepParameterIndex + para.ToString();
					string parameterValue = stringStepParaValue + para.ToString();
					test.Element(parameterType).Value = "";
					test.Element(parameterValue).Value = "";
					if (dataGridViewParameters.Rows.Count > para)
					{
						DataGridViewRow r = dataGridViewParameters.Rows[para];
						string value = (string)r.Cells[1].Value;
						string codename = (string)r.Cells[2].Value;
						if (value == null) value = "";
						value = value.Trim();	// remove the added spaces
						if (codename != null && codename.Length > 0)
						{
							test.Element(parameterType).Value = codename;
							test.Element(parameterValue).Value = value;
						}
					}
				}
#else
				int incorrectSyntax = 0;
				bool incorrectSyntaxButAllowToProceed = false;
				switch (command)
				{
					case stringCommand_SetTesterOutput:
					case stringCommand_BarcodeScan:
					case stringCommand_BarcodeScan_Old:
					case stringCommand_VerifyTesterInput:
					case stringCommand_WaitForTesterInputs:
					case stringCommand_WaitOutput_Camera:
						int valueSet = 0;
						for (int i = 0; i < dataGridViewProbes.Rows.Count; i++)
						{
							DataGridViewRow r = dataGridViewProbes.Rows[i];
							string value = ((string)r.Cells[1].Value).Trim();
							if (value != null && value.Length > 0)
							{
								if (ConvertTextToNumber(value, false, false, false, false) == -1) incorrectSyntax++;
								valueSet++;
							}
						}
						if (valueSet == 0)
						{
							incorrectSyntax++;
							if (command == stringCommand_VerifyTesterInput) incorrectSyntaxButAllowToProceed = true;
						}
						break;
				}
				switch (command)
				{
					case stringCommand_WaitRandom:
					case stringCommand_WaitForTesterInputs:
					case stringCommand_WaitOutput_Camera:
						if (ConvertTextToNumber(textBoxMinDelay.Text.Trim(), false, false, true, false) == -1) incorrectSyntax++;
						if (ConvertTextToNumber(textBoxMaxDelay.Text.Trim(), false, true, true, false) == -1) incorrectSyntax++;
						break;
					case stringCommand_Wait:
						if (ConvertTextToNumber(textBoxMinDelay.Text.Trim(), false, true, true, false) == -1) incorrectSyntax++;
						break;
					case stringCommand_RepeatStart:
						if (ConvertTextToNumber(textBoxMinDelay.Text.Trim(), false, false, false, true) == -1) incorrectSyntax++;
						break;
					case stringCommand_WaitUser:
					case stringCommand_DebugWaitUser:
					case stringCommand_RepeatEnd:
						break;
					case stringCommand_EraseRenesas:
					case stringCommand_EraseCypress:
						if (comboBoxBoard.Text.Trim().Length == 0) incorrectSyntax++;
						break;
					case stringCommand_FlashRenesas:
					case stringCommand_FlashMicrochip:
					case stringCommand_FlashCypress:
					case stringCommand_FlashInventek:
					case stringCommand_ReadUart:
					case stringCommand_SearchUart:
					case stringCommand_TransmitUart:
						if (textBoxHexFile.Text.Trim().Length == 0) incorrectSyntax++;
						if (comboBoxBoard.Text.Trim().Length == 0) incorrectSyntax++;
						break;
					case stringCommand_SubScript:
					case stringCommand_WaitImage:
						if (textBoxHexFile.Text.Trim().Length == 0) incorrectSyntax++;
						//if (ConvertTextToNumber(textBoxMinDelay.Text.Trim(), false, true, true, true) == -1) incorrectSyntax++;
						break;
				}
				if (incorrectSyntax > 0)
				{
					if (incorrectSyntax == 1 && incorrectSyntaxButAllowToProceed)
					{
						if (MessageBox.Show("You intend to verify outputs but don't care about their value. Are you sure to continue?", "Verify Output", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
						{
							return;
						}
					}
					else
					{
						MessageBox.Show("Incorrect or incomplete entry in one of the fields");
						return;
					}
				}
				test.RemoveNodes();
				XElement cmd = new XElement(stringCommandCodeName, command);
				test.Add(cmd);
				XElement alias = new XElement(stringCommandName, GetCommandAlias(command));
				test.Add(alias);
				// time parameter
				switch (command)
				{
					case stringCommand_WaitRandom:
					case stringCommand_WaitForTesterInputs:
					case stringCommand_WaitOutput_Camera:
						{
							string min = textBoxMinDelay.Text.Trim();
							string max = textBoxMaxDelay.Text.Trim();
							int msMin = ConvertTextToNumber(min, false, false, true, false);
							int msMax = ConvertTextToNumber(max, false, false, true, false);
							if (msMin >= 0 && msMax > 0)	// expected, because it was checked before
							{
								XElement v0 = new XElement(stringMinValue, min); test.Add(v0);
								XElement v1 = new XElement(stringMaxValue, max); test.Add(v1);
								XElement ms0 = new XElement(stringMinValue_ForPi, msMin.ToString()); test.Add(ms0);
								XElement ms1 = new XElement(stringMaxValue_ForPi, msMax.ToString()); test.Add(ms1);
								XElement u = new XElement(stringUnit_ForPi, "ms"); test.Add(u);
							}
						}
						break;
					case stringCommand_Wait:
						{
							string min = textBoxMinDelay.Text.Trim();
							int msMin = ConvertTextToNumber(min, false, true, true, false);
							if (msMin > 0)	// expected, because it was checked before
							{
								XElement v0 = new XElement(stringValue, min); test.Add(v0);
								XElement ms0 = new XElement(stringValue_ForPi, msMin.ToString()); test.Add(ms0);
								XElement u = new XElement(stringUnit_ForPi, "ms"); test.Add(u);
							}
						}
						break;
					case stringCommand_SubScript:
						{
							string subScript = textBoxHexFile.Text.Trim();
							if (subScript.Length > 0)	// expected, because it was checked before
							{
								XElement i = new XElement("SubScript", subScript); test.Add(i);
								XElement i_script = new XElement("_Subscript", subScript); test.Add(i_script);
							}
						}
						break;
					case stringCommand_WaitImage:
						{
							string image = textBoxHexFile.Text.Trim();
							if (image.Length > 0)	// expected, because it was checked before
							{
								XElement i = new XElement("Image", image); test.Add(i);
								XElement i_script = new XElement("_Image", image); test.Add(i_script);
							}
						}
						break;
					case stringCommand_RepeatStart:
						{
							string min = textBoxMinDelay.Text.Trim();
							int msMin = ConvertTextToNumber(min, false, false, false, false);
							if (msMin >= 0)	// expected, because it was checked before
							{
								XElement v0 = new XElement(stringRepeatNumber, min); test.Add(v0);
								XElement ms0 = new XElement(stringRepeatNumber_ForPi, msMin.ToString()); test.Add(ms0);
							}
							else if (msMin == -100)	// expected, because it was checked before
							{
								XElement v0 = new XElement(stringRepeatNumber, ""); test.Add(v0);
								XElement ms0 = new XElement(stringRepeatNumber_ForPi, ""); test.Add(ms0);
							}
						}
						break;
					case stringCommand_WaitUser:
					case stringCommand_DebugWaitUser:
					case stringCommand_RepeatEnd:
						break;
				}
				// pin states
				switch (command)
				{
					case stringCommand_EraseRenesas:
					case stringCommand_EraseCypress:
						{
							string boardName = comboBoxBoard.Text;
							if (boardName != null && boardName.Length > 0)
							{
								foreach (XElement obj in testXmlDoc.Descendants(stringBoardOnTester))
								{
									string board = (string)obj.Element(stringBoardIndexOnTester);
									XElement module = SearchModuleByPosition(board);
									if (module != null)
									{
										string name = module.Element(stringModuleAlias).Value;
										if (boardName == GetBoardName(name, board))
										{
											XElement b = new XElement("BoardId", board); test.Add(b);
											XElement n = new XElement("_Board", name); test.Add(n);
											XElement board_addr = new XElement("_BoardAddr", board); test.Add(board_addr);
											break;
										}
									}
								}
							}
						}
						break;
					case stringCommand_FlashRenesas:
					case stringCommand_FlashMicrochip:
					case stringCommand_FlashCypress:
					case stringCommand_FlashInventek:
						{
							string boardName = comboBoxBoard.Text;
							if (boardName != null && boardName.Length > 0)
							{
								foreach (XElement obj in testXmlDoc.Descendants(stringBoardOnTester))
								{
									string board = (string)obj.Element(stringBoardIndexOnTester);
									XElement module = SearchModuleByPosition(board);
									if (module != null)
									{
										string name = module.Element(stringModuleAlias).Value;
										if (boardName == GetBoardName(name, board))
										{
											XElement b = new XElement("BoardId", board); test.Add(b);
											XElement n = new XElement("_Board", name); test.Add(n);
											XElement board_addr = new XElement("_BoardAddr", board); test.Add(board_addr);
											break;
										}
									}
								}
							}
							string hex = textBoxHexFile.Text.Trim();
							if (hex.Length > 0)	// expected, because it was checked before
							{
								if (command == stringCommand_FlashInventek)
								{
									XElement h = new XElement("MacLogFile", hex); test.Add(h);
									XElement h_script = new XElement("_MacLogFile", hex); test.Add(h_script);
								}
								else
								{
									XElement h = new XElement("FirmwareFile", hex); test.Add(h);
									XElement h_script = new XElement("_FirmwareFile", hex); test.Add(h_script);
								}
							}
						}
						break;
					case stringCommand_ReadUart:
					case stringCommand_SearchUart:
					case stringCommand_TransmitUart:
						{
							string boardName = comboBoxBoard.Text;
							if (boardName != null && boardName.Length > 0)
							{
								foreach (XElement obj in testXmlDoc.Descendants(stringBoardOnTester))
								{
									string board = (string)obj.Element(stringBoardIndexOnTester);
									XElement module = SearchModuleByPosition(board);
									if (module != null)
									{
										string name = module.Element(stringModuleAlias).Value;
										if (boardName == GetBoardName(name, board))
										{
											XElement b = new XElement("BoardId", board); test.Add(b);
											XElement n = new XElement("_Board", name); test.Add(n);
											XElement board_addr = new XElement("_BoardAddr", board); test.Add(board_addr);
											break;
										}
									}
								}
							}
							string hex = textBoxHexFile.Text.Trim();
							if (hex.Length > 0)	// expected, because it was checked before
							{
								XElement h = new XElement("MessageString", hex); test.Add(h);
								XElement h_script = new XElement("_MessageString", hex); test.Add(h_script);
							}
						}
						break;
					case stringCommand_SetTesterOutput:
					case stringCommand_BarcodeScan:
					case stringCommand_BarcodeScan_Old:
					case stringCommand_VerifyTesterInput:
					case stringCommand_WaitForTesterInputs:
					case stringCommand_WaitOutput_Camera:
						bool boardWritten = false;
						int channel_num = 0;
						int currentChannel = 0;
						int previousChannel = 0;
						for (int i = 0; i < dataGridViewProbes.Rows.Count; i++)
						{
							DataGridViewRow r = dataGridViewProbes.Rows[i];
							string pin = (string)r.Cells[0].Value;
							string value = (string)r.Cells[1].Value;
							string specialFlameBoardAdditionalChannels = (string)r.Cells[2].Value;
							specialFlameBoardAdditionalChannels = specialFlameBoardAdditionalChannels.Trim();
							string specialPowerBoardAdditionalChannels = (string)r.Cells[3].Value;
							specialPowerBoardAdditionalChannels = specialPowerBoardAdditionalChannels.Trim();
							string specialProgrammingBoardAdditionalChannels = (string)r.Cells[4].Value;
							specialProgrammingBoardAdditionalChannels = specialProgrammingBoardAdditionalChannels.Trim();
							if (value != null)
							{
								value = value.Trim();
								string board = "";
								string channel = "";
								if (pin.Length > 0)
								{
									XElement probe = SearchForProbeElement(pin, "", "");
									if (probe != null)
									{
										board = (string)probe.Element(stringBoardIndexOnTester);
										channel = (string)probe.Element(stringPinNumberOnBoard);
										if (boardWritten == false)
										{
											if (board != null && board.Length > 0 && channel != null && channel.Length > 0)
											{
												XElement b = new XElement("BoardId", board);
												test.Add(b);
												XElement module = SearchModuleByPosition(board);
												if (module != null)
												{
													string name = module.Element(stringModuleAlias).Value;
													XElement n = new XElement("_Board", name);
													test.Add(n);
													XElement board_addr = new XElement("_BoardAddr", board);
													test.Add(board_addr);														
													string totalChannels = "";
													if (command == stringCommand_SetTesterOutput || command == stringCommand_BarcodeScan || command == stringCommand_BarcodeScan_Old)
													{
														totalChannels = stringModuleTotalChannelsOut;
													}
													else totalChannels = stringModuleTotalChannelsIn;
													string total_num = module.Element(totalChannels).Value;
													if (total_num.Length > 0)
													{
														try
														{
															channel_num = Convert.ToInt32(total_num);
														}
														catch
														{
														}
													}
												}
												boardWritten = true;
											}
										}
										if (boardWritten && channel != null && channel.Length > 0)
										{
											previousChannel = currentChannel;
											try
											{
												currentChannel = Convert.ToInt32(channel);
											}
											catch
											{
											}

											switch (command)
											{
												case stringCommand_SetTesterOutput:
												case stringCommand_BarcodeScan:
												case stringCommand_BarcodeScan_Old:
													{
														while (currentChannel > previousChannel + 1)	// add _CH as placeholder but the required value is "don't care"
														{
															previousChannel++;
															string ch = "_CH" + previousChannel.ToString(); /////////////@@@@@@ should all _CH be composed when file is saved? That allows for correction when file is openned
															if (test.Descendants(ch).Count() == 0)
															{
																XElement v = new XElement(ch, "");
																test.Add(v);
															}
														}
														if (specialFlameBoardAdditionalChannels.Length > 0)
														{
															MessageBox.Show("Probably another standard command should be used. Call Tony to review.");
															int new_channel_num = Convert.ToInt32(specialFlameBoardAdditionalChannels);
															XElement newElement =
																	new XElement("Probe", new XAttribute(stringId, currentChannel.ToString("D2")),
																		new XElement("Name", pin),
																		new XElement("BoardId", board),
																		new XElement("ChannelId", channel),
																		new XElement("ChannelNumberInGroup", new_channel_num.ToString()),
																		new XElement(stringValue, value)
																		);
															test.Add(newElement);
															// to expand from one channel to a couple
															string ch = "";
															for (int j = 1; j <= new_channel_num; j++)
															{
																ch = "_CH" + j.ToString();
																string new_value = "0";
																if (j.ToString() == value) new_value = "1";
																if (test.Descendants(ch).Count() == 0)
																{
																	XElement v = new XElement(ch, new_value);
																	test.Add(v);
																}
															}
															// below is the addition for the HSI activation
															ch = "_CH" + (new_channel_num + 1).ToString();
															if (test.Descendants(ch).Count() == 0)
															{
																XElement special_hsi_ele = new XElement(ch, "1");
																test.Add(special_hsi_ele);
															}
														}
														else if (specialPowerBoardAdditionalChannels.Length > 0)
														{
															int new_channel_num = Convert.ToInt32(specialPowerBoardAdditionalChannels);
															XElement newElement =
																	new XElement("Probe", new XAttribute(stringId, currentChannel.ToString("D2")),
																		new XElement("Name", pin),
																		new XElement("BoardId", board),
																		new XElement("ChannelId", channel),
																		new XElement("ChannelNumberInGroup", new_channel_num.ToString()),
																		new XElement(stringValue, value)
																		);
															test.Add(newElement);
															// to expand from one channel to a couple
															string ch = "";
															int this_channel_num = Convert.ToInt32(channel);
															for (int j = 0; j < new_channel_num; j++)
															{
																ch = "_CH" + (j + this_channel_num).ToString();
																string new_value = "0";
																if ((j + 1).ToString() == value) new_value = "1";
																if (test.Descendants(ch).Count() == 0)
																{
																	XElement v = new XElement(ch, new_value);
																	test.Add(v);
																}
															}
														}
														else
														{
															XElement newElement =
																	new XElement("Probe", new XAttribute(stringId, currentChannel.ToString("D2")),
																		new XElement("Name", pin),
																		new XElement("BoardId", board),
																		new XElement("ChannelId", channel),
																		new XElement(stringValue, value)
																		);
															test.Add(newElement);
															string ch = "_CH" + channel;
															if (test.Descendants(ch).Count() == 0)
															{
																XElement v = new XElement(ch, value);
																test.Add(v);
															}
														}
													}
													break;
												case stringCommand_VerifyTesterInput:
												case stringCommand_WaitForTesterInputs:
												case stringCommand_WaitOutput_Camera:
													{
														while (currentChannel > previousChannel + 1)	// add _CH as placeholder but the required value is "don't care"
														{
															previousChannel++;
															string ch_0 = "_CH" + previousChannel.ToString() + "Min";
															if (test.Descendants(ch_0).Count() == 0)
															{
																XElement v_0 = new XElement(ch_0, "0");
																test.Add(v_0);
															}
															string ch_1 = "_CH" + previousChannel.ToString() + "Max";
															if (test.Descendants(ch_1).Count() == 0)
															{
																XElement v_1 = new XElement(ch_1, "1");
																test.Add(v_1);
															}
														}
														if (specialFlameBoardAdditionalChannels.Length > 0)
														{
															MessageBox.Show("Probably another standard command should be used. Call Tony to review.");
														}
														else if (specialPowerBoardAdditionalChannels.Length > 0)
														{
															int new_channel_num = Convert.ToInt32(specialPowerBoardAdditionalChannels);
															XElement newElement =
																	new XElement("Probe", new XAttribute(stringId, currentChannel.ToString("D2")),
																		new XElement("Name", pin),
																		new XElement("BoardId", board),
																		new XElement("ChannelId", channel),
																		new XElement("ChannelNumberInGroup", new_channel_num.ToString()),
																		new XElement(stringValue, value)
																		);
															test.Add(newElement);
															// to expand from one channel to a couple
															int this_channel_num = Convert.ToInt32(channel);
															for (int j = 0; j < new_channel_num; j++)
															{
																string ch0 = "_CH" + (j + this_channel_num).ToString() + "Min";
																string value0 = "0";
																if ((j + 1).ToString() == value) value0 = "1";
																if (test.Descendants(ch0).Count() == 0)
																{
																	XElement v0 = new XElement(ch0, value0);
																	test.Add(v0);
																}
																string ch1 = "_CH" + (j + this_channel_num).ToString() + "Max";
																string value1 = "0";
																if (value.Length == 0 || (j + 1).ToString() == value) value1 = "1";
																if (test.Descendants(ch1).Count() == 0)
																{
																	XElement v1 = new XElement(ch1, value1);
																	test.Add(v1);
																}
															}
														}
														else if (specialProgrammingBoardAdditionalChannels.Length > 0)
														{
															int new_channel_num = Convert.ToInt32(specialProgrammingBoardAdditionalChannels);
															XElement newElement =
																	new XElement("Probe", new XAttribute(stringId, currentChannel.ToString("D2")),
																		new XElement("Name", pin),
																		new XElement("BoardId", board),
																		new XElement("ChannelId", channel),
																		new XElement("ChannelNumberInGroup", new_channel_num.ToString()),
																		new XElement(stringValue, value)
																		);
															test.Add(newElement);
															// to expand from one channel to a couple
															string ch = "";
															int this_channel_num = Convert.ToInt32(channel);
															int valueDec = 0;
															if (value.Length > 0)
															{
																try
																{
																	valueDec = Convert.ToInt32(value);
																}
																catch (Exception ex)
																{
																	MessageBox.Show(ex.Message);
																}
															}
															for (int j = 0; j < new_channel_num; j++)
															{
																string ch0 = "_CH" + (j + this_channel_num).ToString() + "Min";
																string value0 = "0";
																if ((valueDec & (1 << j)) > 0) value0 = "1";
																if (test.Descendants(ch0).Count() == 0)
																{
																	XElement v0 = new XElement(ch0, value0);
																	test.Add(v0);
																}
																string ch1 = "_CH" + (j + this_channel_num).ToString() + "Max";
																string value1 = "0";
																if (value.Length == 0 || (valueDec & (1 << j)) > 0) value1 = "1";
																if (test.Descendants(ch1).Count() == 0)
																{
																	XElement v1 = new XElement(ch1, value1);
																	test.Add(v1);
																}
															}
														}
														else
														{
															XElement newElement =
																	new XElement("Probe", new XAttribute(stringId, currentChannel.ToString("D2")),
																		new XElement("Name", pin),
																		new XElement("BoardId", board),
																		new XElement("ChannelId", channel),
																		new XElement(stringValue, value)
																		);
															test.Add(newElement);
															string ch0 = "_CH" + channel + "Min";
															string value0 = "0";
															if (value.Length > 0) value0 = value;
															if (test.Descendants(ch0).Count() == 0)
															{
																XElement v0 = new XElement(ch0, value0);
																test.Add(v0);
															}
															string ch1 = "_CH" + channel + "Max";
															string value1 = "1";
															if (value.Length > 0) value1 = value;
															if (test.Descendants(ch1).Count() == 0)
															{
																XElement v1 = new XElement(ch1, value1);
																test.Add(v1);
															}
														}
													}
													break;
											}
										}
									}
								}
							}
						}
						for (int i = currentChannel + 1; i <= channel_num; i++)
						{
							switch (command)
							{
								case stringCommand_SetTesterOutput:
								case stringCommand_BarcodeScan:
								case stringCommand_BarcodeScan_Old:
									{
										string ch = "_CH" + i.ToString();
										if (test.Descendants(ch).Count() == 0)
										{
											XElement v = new XElement(ch, "");
											test.Add(v);
										}
									}
									break;
								case stringCommand_VerifyTesterInput:
								case stringCommand_WaitRandom:
								case stringCommand_WaitForTesterInputs:
								case stringCommand_WaitOutput_Camera:
									{
										string ch_0 = "_CH" + i.ToString() + "Min";
										if (test.Descendants(ch_0).Count() == 0)
										{
											XElement v_0 = new XElement(ch_0, "0");
											test.Add(v_0);
										}
										string ch_1 = "_CH" + i.ToString() + "Max";
										if (test.Descendants(ch_1).Count() == 0)
										{
											XElement v_1 = new XElement(ch_1, "1");
											test.Add(v_1);
										}
									}
									break;
							}
						}
						break;
				}
				XElement note = new XElement(stringTestStepNote, textBoxTestName.Text.Trim());
				test.Add(note);
				if (ToDisable)
				{
					XElement enabled = new XElement(stringTestStepDisabled, "1");
					test.Add(enabled);
				}
#endif
				LoadTestFile(id);
			}
		}

		private void listViewTestSteps_SelectedIndexChanged(object sender, EventArgs e)
		{
			labelProbes.Text = "";
			int id = GetSelectedTest();
			if (id >= 1)
			{
				XElement test = SearchElementById(stringStep, id);
				if (test != null)
				{
#if	old_scheme
					dataGridViewParameters.Rows.Clear();
					comboBoxTestObject.Items.Clear();
					comboBoxTestCommand.Items.Clear();
					string obj = test.Element(stringConnectionAlias).Value;
					if (obj != null && obj.Length > 0)
					{
						XElement probe = SearchForProbeElement(obj, "", "");
						if (probe != null)
						{
							XElement pin = SearchPinByProbeName(obj);
							if (pin != null)
							{
								string direction = pin.Element(stringIoDirection).Value;
								if (direction != null && direction.Length > 0)
								{
									foreach (string testType in comboBoxTestType.Items)
									{
										string ioDirection = "";
										switch (testType[testType.Length - 2])
										{
											case 'O':
												ioDirection = stringIoDirection_Input;
												break;
											case 'I':
												ioDirection = stringIoDirection_Output;
												break;
											case 'T':
												ioDirection = stringIoDirection_Input;
												break;
											default:	// 'W'
												ioDirection = "W";
												break;
										}
										if (ioDirection == direction)
										{
											comboBoxTestType.Text = testType;
											break;
										}
									}
								}
							}
						}
						comboBoxTestObject.Items.Add(obj);
						comboBoxTestObject.SelectedIndex = 0;
					}
					else
					{
						foreach (string testType in comboBoxTestType.Items)
						{
							string ioDirection = "";
							switch (testType[testType.Length - 2])
							{
								case 'O':
								case 'I':
								case 'T':
									break;
								default:	// 'W'
									ioDirection = "W";
									break;
							}
							if (ioDirection.Length > 0)
							{
								comboBoxTestType.Text = testType;
								break;
							}
						}
					}
					string cmd = test.Element(stringCommandCodeName).Value;
					if (cmd != null && cmd.Length > 0)
					{
						string alias = SearchCommandAliasByCodeName(cmd);
						comboBoxTestCommand.Items.Add(alias);
						comboBoxTestCommand.SelectedIndex = 0;
						//parameters
						for (int para = 0; para < MAX_PARAMETERS; para++)
						{
							string parameterIndex = stringStepParameterIndex + para.ToString();
							string parameterValue = stringStepParaValue + para.ToString();
							string parameterCodeName = test.Element(parameterIndex).Value;
							if (parameterCodeName != null && parameterCodeName.Length > 0)
							{
								string value = "";
									
								value = test.Element(parameterValue).Value;
								if (value == null) value = "";
								value += "                  ";
								DataGridViewRow r = new DataGridViewRow();
								r.CreateCells(dataGridViewParameters);
								r.Cells[0].Value = SearchParameterAliasByCodeName(cmd, parameterCodeName, stringParameterAlias);
								r.Cells[1].Value = value;
								r.Cells[2].Value = parameterCodeName;
								r.Cells[3].Value = SearchParameterAliasByCodeName(cmd, parameterCodeName, stringParameterValueMin);
								r.Cells[4].Value = SearchParameterAliasByCodeName(cmd, parameterCodeName, stringParameterValueMax);
								dataGridViewParameters.Rows.Add(r);
							}
						}
					}
#else
					HideCommandDetails();
					string command = test.Element(stringCommandCodeName).Value;
					if (command != null && command.Length > 0)
					{
						string cmdAlias = SearchCommandAliasByCodeName(command);
						if (cmdAlias.Length > 0)
						{
							comboBoxTestType.Text = cmdAlias;
							PopulateCommandDetails("");
						}
					}		
#endif
				}
			}
		}

		private string GetCurrentSelectedCommand()
		{
			string testType = comboBoxTestType.Text.Trim();
#if old_scheme
			string cmdAlias = SearchCommandAliasByCodeName(command);
			if (testType != null && testType.Length > 3)
			{
				switch (testType[testType.Length - 2])
				{
					case 'O':
						return stringIoDirection_Output;
					case 'I':
						return stringIoDirection_Input;
					case 'T':
						return "T";
					case 'W':
						return "W";
				}
			}
			return "";
#else
			string cmd = SearchCommandCodeNameByAlias("", testType);
			return cmd;
#endif
		}

		private string GetCurrentSelectedCommandIoDirection()
		{
			string command = GetCurrentSelectedCommand();
			switch (command)
			{
				case stringCommand_VerifyTesterInput:
				case stringCommand_WaitForTesterInputs:
				case stringCommand_WaitOutput_Camera:
					return stringIoDirection_Input;
				case stringCommand_SetTesterOutput:
				case stringCommand_BarcodeScan:
				case stringCommand_BarcodeScan_Old:
					return stringIoDirection_Output;
			}
			return "";
		}

		private string GetCommandAlias(string command)
		{
			switch (command)
			{
				case stringCommand_VerifyTesterInput:
					return "SetInputs";
				case stringCommand_SetTesterOutput:
					return "SetOutputs";
				case stringCommand_BarcodeScan:
				case stringCommand_BarcodeScan_Old:
					return "BarcodeScan";
				case stringCommand_WaitRandom:
					return "WaitRandom";
				case stringCommand_WaitForTesterInputs:
					return "WaitTill";
				case stringCommand_WaitOutput_Camera:
					return "WaitTillCamera";
				case stringCommand_Wait:
					return "Wait";
				case stringCommand_WaitUser:
				case stringCommand_DebugWaitUser:
					return "WaitForUserInput";
				case stringCommand_RepeatEnd:
					return "RepeatEnd";
				case stringCommand_RepeatStart:
					return "RepeatStart";
				case stringCommand_FlashRenesas:
					return "FlashRenesas";
				case stringCommand_FlashMicrochip:
					return "FlashMicrochip";
				case stringCommand_FlashCypress:
					return "FlashCypress";
				case stringCommand_FlashInventek:
					return "FlashInventek";
				case stringCommand_EraseRenesas:
					return "EraseRenesas";
				case stringCommand_EraseCypress:
					return "EraseCypress";
				case stringCommand_ReadUart:
					return "UartRead";
				case stringCommand_SearchUart:
					return "UartSearch";
				case stringCommand_TransmitUart:
					return "UartTransmit";
				case stringCommand_SubScript:
					return "SubScript";
				case stringCommand_WaitImage:
					return "WaitForImageInstr";
			}
			return "";
		}

		private void comboBoxTestObject_DropDown(object sender, EventArgs e)
		{
#if old_scheme
			comboBoxTestObject.Items.Clear();
			if (testXmlDoc != null)
			{
				string ioDirection = GetCurrentSelectedCommandIoDirection();
				foreach (XElement obj in testXmlDoc.Descendants(stringConnection))
				{
					string str = (string)obj.Element(stringConnectionAlias);
					if (ioDirection == stringIoDirection_Input || ioDirection == stringIoDirection_Output)
					{
						string board = (string)obj.Element(stringBoardIndexOnTester);
						string pinNumber = (string)obj.Element(stringPinNumberOnBoard);
						XElement pin = SearchPinById(board, pinNumber);
						if (pin != null)
						{
							string direction = pin.Element(stringIoDirection).Value;
							if (direction != null && direction.Length > 0)
							{
								if (direction == ioDirection)
								{
									comboBoxTestObject.Items.Add(str);
								}
							}
						}
					}
					else if (ioDirection == "")
					{
						comboBoxTestObject.Items.Add(str);
					}
					else if (ioDirection == "W")
					{
					}
				}
			}
#endif
		}

		private void comboBoxTestCommand_DropDown(object sender, EventArgs e)
		{
#if old_scheme
			comboBoxTestCommand.Items.Clear();
			if (testXmlDoc != null)
			{
				// depends on comboBoxTestObject.Text
				if (comboBoxTestObject.Text.Length > 0)
				{
					XElement pin = SearchPinByProbeName(comboBoxTestObject.Text);
					if (pin != null)
					{
						foreach (XElement cmd in pin.Descendants(stringApplicableCommand))
						{
							string commandId = (string)cmd.Element(stringCommandCodeName);
							if (commandId != null && commandId.Length > 0)
							{
								XElement command = SearchCommandByCodeName(commandId);
								if (command != null)
								{
									string commandName = (string)command.Element(stringCommandAlias);
									if (commandName != null && commandName.Length > 0)
									{
										comboBoxTestCommand.Items.Add(commandName);
									}
								}
							}
						}
					}
				}
				else
				{
					string c = comboBoxTestType.Text[comboBoxTestType.Text.Length - 2].ToString();
					XElement command = SearchCommandByCodeName(c.ToString());
					if (command != null)
					{
						string commandName = (string)command.Element(stringCommandAlias);
						if (commandName != null && commandName.Length > 0)
						{
							comboBoxTestCommand.Items.Add(commandName);
						}
					}
				}
			}
#endif
		}

		private void listViewTestSteps_MouseClick(object sender, MouseEventArgs e)
		{
			listViewTestSteps_SelectedIndexChanged(sender, e);
		}

		private void treeViewConnections_DoubleClick(object sender, EventArgs e)
		{
			buttonAddConnection_Click(sender, e);
		}

		private void buttonAddConnection_Click(object sender, EventArgs e)
		{
			using (FormTextInput dlg = new FormTextInput())
			{
				TreeNode node = treeViewConnections.SelectedNode;
				if (node != null)
				{
					TreeNode parent = node.Parent;
					if (parent != null)
					{
						string boardIndexOnTester = (string)parent.Tag;
						string pinNumberOnBoard = (string)node.Tag;
						XElement obj = SearchForProbeElement("", boardIndexOnTester, pinNumberOnBoard);
						if (obj == null)
						{
							XElement pin = SearchPinById(boardIndexOnTester, pinNumberOnBoard);
							if (pin != null)
							{
								string ioDirection = (string)pin.Element(stringIoDirection).Value;
								if (ioDirection == "-")
								{
									MessageBox.Show("This is a reference pin without a specific function.");
									return;
								}
								if (pin.Element(stringPinAlias).Value == ".")
								{
									MessageBox.Show("This pin is grouped together with the preceeding pin(s) to perform a function.\nYou don't need to name it.");
									return;
								}
							}
							DialogResult result = dlg.ShowDialog();
							switch (result)
							{
								case System.Windows.Forms.DialogResult.OK:
									string newName = dlg.textBoxInput.Text.Trim();
									if (newName.Length > 0)
									{
										// check if replacedByNewAlias is currently used, proceed if only not
										if (SearchForProbeElement(newName, "", "") == null)
										{
											AddProbe(newName, boardIndexOnTester, pinNumberOnBoard);
											ShowTesterConnections(boardIndexOnTester, pinNumberOnBoard);
										}
										else
										{
											MessageBox.Show("Probe name \"" + newName + "\" is already used.");
										}
									}
									break;
							}
						}
					}
				}
			}
		}

		private void buttonRenameConnection_Click(object sender, EventArgs e)
		{
			using (FormTextInput dlg = new FormTextInput())
			{
				TreeNode node = treeViewConnections.SelectedNode;
				if (node != null)
				{
					TreeNode parent = node.Parent;
					if (parent != null)
					{
						string board = (string)parent.Tag;
						string channel = (string)node.Tag;
						XElement obj = SearchForProbeElement("", board, channel);
						if (obj != null)
						{
							string oldName = (string)obj.Element(stringConnectionAlias);
							if (oldName != null && oldName.Length > 0)
							{
								dlg.textBoxInput.Text = oldName;
								DialogResult result = dlg.ShowDialog();
								switch (result)
								{
									case System.Windows.Forms.DialogResult.OK:
										string newName = dlg.textBoxInput.Text.Trim();
										if (newName.Length > 0 && newName != oldName)
										{
											// check if replacedByNewAlias is currently used, proceed if only not
											if (SearchForProbeElement(newName, "", "") == null)
											{
												FindOrReplaceTestConnectionAlias(oldName, newName);
												obj.Element(stringConnectionAlias).Value = newName;
												ShowTesterConnections(board, channel);
												ShowTestSteps();
											}
											else
											{
												MessageBox.Show("Probe name \"" + newName + "\" is already used.");
											}
										}
										break;
								}
							}
						}
					}
				}
			}
		}

		private void buttonReconnect_Click(object sender, EventArgs e)
		{
			using (FormTestConnection dlg = new FormTestConnection())
			{
				TreeNode node = treeViewConnections.SelectedNode;
				if (node != null)
				{
					TreeNode parent = node.Parent;
					if (parent != null)
					{
						string boardIndexOnTester = (string)parent.Tag;
						string pinNumberOnBoard = (string)node.Tag;
						XElement obj = SearchForProbeElement("", boardIndexOnTester, pinNumberOnBoard);
						if (obj != null)
						{
							if (testXmlDoc != null)
							{
								for (int i = 1; ; i++)
								{
									string position = i.ToString();
									XElement board = SearchBoardByPosition(position);
									if (board != null)
									{
										dlg.comboBoxBoard.Items.Add(position);
									}
									else
									{
										break;
									}
								}
							}
							dlg.comboBoxBoard.Text = boardIndexOnTester;
							dlg.comboBoxChannel.Text = pinNumberOnBoard;
							DialogResult result = dlg.ShowDialog();
							switch (result)
							{
								case System.Windows.Forms.DialogResult.OK:
									string board = dlg.comboBoxBoard.Text;
									string newPinNumber = dlg.comboBoxChannel.Text;
									if (board.Length > 0 && newPinNumber.Length > 0)
									{
										XElement pin = SearchPinById(board, newPinNumber);
										if (pin != null)
										{
											string ioDirection = (string)pin.Element(stringIoDirection).Value;
											if (ioDirection == "-")
											{
												MessageBox.Show("This is a reference pin without a specific function.");
											}
											else
											{
												// check if board is currently used, proceed if only not
												if (SearchForProbeElement("", board, newPinNumber) == null)
												{
													obj.Element(stringBoardIndexOnTester).Value = board;
													obj.Element(stringPinNumberOnBoard).Value = newPinNumber;
													ShowTesterConnections(board, newPinNumber);
												}
												else
												{
													MessageBox.Show("The pin you selected\n(board #" + board + ", pin #" + newPinNumber + ")\nis already used.");
												}
											}
										}
										else
										{
											MessageBox.Show("The pin you selected (pin #" + newPinNumber + ") is not available on board #" + board + ".");
										}
									}
									break;
							}
						}
					}
				}
			}
		}

		private void buttonRemoveConnection_Click(object sender, EventArgs e)
		{
			TreeNode node = treeViewConnections.SelectedNode;
			if (node != null)
			{
				TreeNode parent = node.Parent;
				if (parent != null)
				{
					string board = (string)parent.Tag;
					string channel = (string)node.Tag;
					XElement obj = SearchForProbeElement("", board, channel);
					if (obj != null)
					{
						string alias = (string)obj.Element(stringConnectionAlias);
						if (alias != null && alias.Length > 0)
						{
							// check if probe is currently used by test, proceed if only not
							if (FindOrReplaceTestConnectionAlias(alias, "") == false)
							{
								if (MessageBox.Show("Are you sure to remove the probe?", "Remove Probe", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
								{
									obj.Remove();
									ShowTesterConnections(board, channel);
								}
							}
							else
							{
								MessageBox.Show("Probe \"" + alias + "\" cannot be removed because it is currently used in the test procedure.");
							}
						}
					}
				}
			}
		}

		private void toolStripMenuItem2_Click(object sender, EventArgs e)
		{
			tabControl1.SelectedTab = tabPageProbeSetup;
		}

		private void testProcedureToolStripMenuItem_Click(object sender, EventArgs e)
		{
			tabControl1.SelectedTab = tabPageTestProcedure;
		}

		private void moduleBoardsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			tabControl1.SelectedTab = tabPageModuleFunctionality;
		}

		private void boardSetupToolStripMenuItem_Click(object sender, EventArgs e)
		{
			tabControl1.SelectedTab = tabPageBoardSetup;
		}

		private void VerifyBoardSetup()
		{
			if (testXmlDoc != null)
			{
				comboBoxBoard.Items.Clear();
				XElement m = SearchModuleByPosition("1");
				if (m == null)	// for older files, board setup were not embedded in the file. Retrieve it from the tester config file and embed it in the file 
				{
					if (stringTesterConfigFile.Length > 0 && File.Exists(stringTesterConfigFile))
					{
						XDocument testerConfigXmlDoc = XDocument.Load(stringTesterConfigFile);	// load it
						if (testerConfigXmlDoc != null)
						{
							var info = from element in testerConfigXmlDoc.Descendants(stringBoardOnTester)
									   select element;
							if (info != null && info.Count() > 0)
							{
								foreach (XElement element in info.Distinct())
								{
									testXmlDoc.Descendants(stringSetup).First().Add(element);
								}
							}
						}
					}
				}
				bool boardSetupInTestFile = false;
				for (int i = 1; ; i++)
				{
					XElement module = SearchModuleByPosition(i.ToString());
					if (module != null)
					{
						string name = GetBoardName(module.Element(stringModuleAlias).Value, i.ToString());
						comboBoxBoard.Items.Add(name);
						boardSetupInTestFile = true;
					}
					else
					{
						break;
					}
				}
				if (boardSetupInTestFile == false)
				{
					MessageBox.Show("Error: Board setup information not found.");
				}
			}
		}

		private void newToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (stringNewTestFile.Length > 0 && File.Exists(stringNewTestFile))
			{
				testXmlDoc = XDocument.Load(stringNewTestFile);	// load it
				currentFile = "";
				if (testXmlDoc != null)
				{
					Cursor.Current = Cursors.WaitCursor;
					VerifyBoardSetup();
					AddTestStep(1);
					ShowTestSteps();
					ShowTestSettings();
					ShowTesterConnections("", "");
					HideCommandDetails();
				}
			}
		}

		const int AUTO_CORRECT_VERSION = 1;

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "ICM Universal Tester files (*.iut)|*.iut|All files (*.*)|*.*";
			dlg.DefaultExt = ".iut";
			DialogResult result = dlg.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				string file = dlg.FileName;
				if (file.Length > 0 && File.Exists(file))
				{
					try
					{
						bool bCheckError = true;
						while (true)
						{
							testXmlDoc = XDocument.Load(file);	// load it
							if (testXmlDoc != null)
							{
								currentFile = file;
								Cursor.Current = Cursors.WaitCursor;
								VerifyBoardSetup();
								if (bCheckError)
								{
									if (testXmlDoc.Descendants(stringScriptAutoCorrectVersion).Count() == 0 ||
										Convert.ToInt32(testXmlDoc.Descendants(stringSetup).First().Element(stringScriptAutoCorrectVersion).Value) < AUTO_CORRECT_VERSION)
									{
										int bChangedMade1 = ReArrangeProbeOrder();
										int bChangedMade2 = CorrectManualEditErrors();
										if (bChangedMade1 > 0 || bChangedMade2 > 0)
										{
											if (testXmlDoc.Descendants(stringScriptAutoCorrectVersion).Count() == 0)
											{
												XElement newElement = new XElement(stringScriptAutoCorrectVersion, "1");
												testXmlDoc.Descendants(stringSetup).First().Add(newElement);
											}
											string str = "This test script includes " + (bChangedMade1 + bChangedMade2).ToString() + " errors. It may still work in the testers but might not work properly with the script editor.\n\nDo you want to automatically correct the the script?";
											if (MessageBox.Show(str, "Accept Automatic Correction", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
											{
												// back up the old file
												string backupFile = file + ".backup.iut";
												File.Copy(file, backupFile, true);
												// save the file as a new file then open the new file
												SaveTestFile(file);
												MessageBox.Show("Test script has been updated. Original script has been saved as\n" + backupFile);
											}
											bCheckError = false;
											continue;
										}
									}
								}
								ShowTestSteps();
								ShowTestSettings();
								ShowTesterConnections("", "");
							}
							break;
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.ToString());
					}
				}
			}
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (currentFile.Length > 0)
			{
				SaveTestFile(currentFile);
			}
			else
			{
				saveAsToolStripMenuItem_Click(sender, e);
			}
		}

		private void SaveTestFile(string filePath)
		{
			if (testXmlDoc != null)
			{
				foreach (var g in testXmlDoc.Descendants(stringSetup))
				{
					var v = g.Elements(stringStep).ToList();
					v.Remove();
					g.Add(v.OrderBy(c => c.Attribute(stringId).Value));
				}
				XElement barcode = SearchElementByTabName(stringBarCode);
				if (barcode != null) barcode.Remove();
				XElement datalog = SearchElementByTabName(stringDatalog);
				if (datalog != null) datalog.Remove();
				XElement noStopAtFailure = SearchElementByTabName(stringNoStopAtFailure);
				if (noStopAtFailure != null) noStopAtFailure.Remove();
#if old_scheme
				for (int i = 1; ; i++)
				{
					XElement test = SearchElementById(stringStep, i);
					if (test != null)
					{
						string id = i.ToString("D4");
						string name = (string)test.Element(stringTestStepNote);
						if (name == null) name = "";
						string command = (string)test.Element(stringCommandCodeName);
						if (command == null) command = "";
						string obj = (string)test.Element(stringConnectionAlias);
						if (obj == null) obj = "";
						string board = "";
						string channel = "";
						if (obj.Length > 0)
						{
							XElement probe = SearchForProbeElement(obj, "", "");
							if (probe != null)
							{
								board = (string)probe.Element(stringBoardIndexOnTester);
								channel = (string)probe.Element(stringPinNumberOnBoard);
							}
						}
						string script = id + ";" + board + ";" + channel + ";" + command + ";";
						for (int para = 0; para < MAX_PARAMETERS; para++)
						{
							string parameterType = stringStepParameterIndex + para.ToString();
							string parameterValue = stringStepParaValue + para.ToString();
							string type = (string)test.Element(parameterType);
							string value = (string)test.Element(parameterValue);
							if (type != null && type.Length > 0 && value != null && value.Length > 0)
							{
								script += type + "=" + value + ";";
							}
						}
						script += name;
						XElement note = new XElement(stringScript, script);
						test.Add(note); ;
					}
					else
					{
						break;
					}
				}
#endif
				if (checkBoxBarcode.Checked)
				{
					XElement newElement = new XElement(stringBarCode, "1");
					testXmlDoc.Descendants(stringSetup).First().Add(newElement);
				}
				if (checkBoxDataLoggerMode.Checked)
				{
					XElement newElement = new XElement(stringDatalog, "1");
					testXmlDoc.Descendants(stringSetup).First().Add(newElement);
				}
				if (checkBoxNoStop.Checked)
				{
					XElement newElement = new XElement(stringNoStopAtFailure, "1");
					testXmlDoc.Descendants(stringSetup).First().Add(newElement);
				}
				for (int i = 1; ; i++)
				{
					XElement test = SearchElementById(stringStep, i);
					if (test != null)
					{
						string command = "";
						XElement cmd = test.Element(stringCommandCodeName);
						if (cmd != null) command = cmd.Value;
						if (command != null && (command == stringCommand_SetTesterOutput || command == stringCommand_BarcodeScan || command == stringCommand_BarcodeScan_Old))
						{
							for (int j = 1; j < 1000; j++)	// a big number that will be exit before it is reached
							{
								string ch = "_CH" + j.ToString();
								XElement ele = test.Element(ch);
								if (ele != null)
								{
									string v = (string)ele.Value;
									if (v.Length == 0)
									{
										string prevV = "0";
										string board = (string)test.Element("BoardId");
										if (board != null && board.Length > 0)
										{
											for (int k = i - 1; k >= 1; k--)
											{
												XElement prevTest = SearchElementById(stringStep, k);
												if (prevTest != null)
												{
													string prevBoard = (string)prevTest.Element("BoardId");
													if (board == prevBoard)
													{
														XElement prevEle = prevTest.Element(ch);
														if (prevEle != null)
														{
															prevV = (string)prevEle.Value;
															break;
														}
													}
												}
											}
											ele.Value = prevV;
										}
									}
								}
								else
								{
									break;
								}
							}
						}
					}
					else
					{
						break;
					}
				}
				testXmlDoc.Save(filePath);
				currentFile = filePath;
			}
		}

		private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.FileName = currentFile;
			dlg.Filter = "ICM Universal Tester files (*.iut)|*.iut|All files (*.*)|*.*";
			dlg.DefaultExt = ".iut";
			DialogResult result = dlg.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				if (dlg.FileName.Length > 0)
				{
					SaveTestFile(dlg.FileName);
				}
			}
		}

		private void printToolStripMenuItem_Click(object sender, EventArgs e)
		{
			string newFileName = currentFile;
			if (tabControl1.SelectedTab == tabPageBoardSetup)
			{
				newFileName += " - Tester Setup";
			}
			else if (tabControl1.SelectedTab == tabPageProbeSetup)
			{
				newFileName += " - Probe Setup";
			}
			else if (tabControl1.SelectedTab == tabPageTestProcedure)
			{
				newFileName += " - Test Procedure";
			}
			newFileName += ".xls";
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.FileName = newFileName;
			dlg.Filter = "Excel files (*.xls)|*.xls|All files (*.*)|*.*";
			dlg.DefaultExt = ".xls";
			DialogResult result = dlg.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				dlg.FileName = dlg.FileName.Trim();
				if (dlg.FileName.Length > 0)
				{
					if (currentFile.ToLower() == dlg.FileName.ToLower())
					{
						MessageBox.Show("You must use a different file name.");
						return;
					}
					if (tabControl1.SelectedTab == tabPageBoardSetup)
					{
						ShowTesterConnections("", "");
						SaveReportDataToExcelFile(dlg.FileName, excelReportBoards);
					}
					else if (tabControl1.SelectedTab == tabPageProbeSetup)
					{
						ShowTesterConnections("", "");
						SaveReportDataToExcelFile(dlg.FileName, excelReportConnections);
					}
					else if (tabControl1.SelectedTab == tabPageTestProcedure)
					{
						ShowTestSteps();
						SaveReportDataToExcelFile(dlg.FileName, excelReportTestSteps);
					}
				}
			}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			//if (currentFile 
		}

		ArrayList excelReportBoards = new ArrayList();
		ArrayList excelReportConnections = new ArrayList();
		ArrayList excelReportTestSteps = new ArrayList();
		

		private bool SaveReportDataToExcelFile(string file, ArrayList exportExcel)
		{
			Cursor.Current = Cursors.WaitCursor;
			int success = 0;
			Microsoft.Office.Interop.Excel.Application xlApp = null;
			Microsoft.Office.Interop.Excel.Workbook xlWorkBook = null;
			Microsoft.Office.Interop.Excel.Worksheet xlWorkSheet = null;
			try
			{
				xlApp = new Microsoft.Office.Interop.Excel.Application();
				if (xlApp != null)
				{
					xlWorkBook = xlApp.Workbooks.Add(Missing.Value);
					if (xlWorkBook != null)
					{
						xlApp.DisplayAlerts = false;
						try
						{
							xlWorkBook.SaveAs(file,
								Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookNormal,
								Missing.Value, Missing.Value, Missing.Value, Missing.Value,
								Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive,
								Missing.Value, Missing.Value, Missing.Value, Missing.Value,
								Missing.Value);
							success = 1;
						}
						catch (Exception ex)
						{
							MessageBox.Show(ex.Message);
						}
						xlApp.DisplayAlerts = true;
					}
					if (xlWorkBook != null && success == 1)
					{
						xlWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
						if (xlWorkSheet != null)
						{
							int excelCurrentRow = 0;
							foreach (string s in exportExcel)
							{
								string str = s;
								excelCurrentRow++;
								int col = 1;
								while (str.Length > 0)
								{
									string newText;
									int index = str.IndexOf('\t');
									if (index >= 0)
									{
										newText = str.Substring(0, index);
										str = str.Substring(index + 1);
									}
									else
									{
										newText = str;
										str = "";
									}
									xlWorkSheet.Cells[excelCurrentRow, col] = newText;
									col++;
								}
							}
							success = 2;
						}
						if (success == 2)
						{
							//xlWorkSheet.Range["A1:K4"].Rows.Font.Bold = true;
							//xlWorkSheet.Range["A4:K4"].Rows.Font.Underline = true;
							//xlWorkSheet.Range["A2"].HorizontalAlignment = HorizontalAlignment.Left;
							//xlWorkSheet.Rows.HorizontalAlignment = HorizontalAlignment.Left;
							xlWorkSheet.Columns.AutoFit();

							xlWorkBook.Save();
							xlWorkBook.Close(true, Missing.Value, Missing.Value);
							success = 3;
						}
					}
				}
				if (xlApp != null) xlApp.Quit();
			}
			finally
			{
				releaseObject(xlWorkSheet);
				releaseObject(xlWorkBook);
				releaseObject(xlApp);
			}
			Cursor.Current = Cursors.Default;
			if (success == 3)
			{
				viewExternalFile("", file);
				return true;
			}
			else return false;
		}

		private void releaseObject(object obj)
		{
			try
			{
				System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
				obj = null;
			}
			catch
			{
				obj = null;
			}
			finally
			{
				GC.Collect();
			}
		}

		private void viewExternalFile(string program, string file)
		{
			Cursor.Current = Cursors.WaitCursor;
			if (program.Length > 0)
			{
				try	// first attempt
				{
					Process newProcess = new Process();
					newProcess = System.Diagnostics.Process.Start(program, file);
					return;
				}
				catch (System.ComponentModel.Win32Exception ex)
				{
					if (ex.Message.StartsWith("The system cannot find the file specified") == true)
					{
						try	// second attempt
						{
							Process newProcess2 = new Process();
							newProcess2 = System.Diagnostics.Process.Start(file);
						}
						catch (Exception ex2)
						{
							MessageBox.Show(ex2.Message);
						}
					}
					else
					{
						MessageBox.Show(ex.Message);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
			}
			else
			{
				try
				{
					Process newProcess = new Process();
					newProcess = System.Diagnostics.Process.Start(file);
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
			}
			Cursor.Current = Cursors.Default;
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			exitToolStripMenuItem_Click(sender, e);
		}

		string comboBoxTestTypeBackup = "";

		private void comboBoxTestType_DropDownClosed(object sender, EventArgs e)
		{
			int id = GetSelectedTest();
			if (id >= 1)
			{
				if (comboBoxTestTypeBackup != comboBoxTestType.Text)
				{
					comboBoxTestObject_DropDown(sender, e);
					comboBoxTestCommand_DropDown(sender, e);
				}
				PopulateCommandDetails("");
			}
			else
			{
				MessageBox.Show("Please first select the test step which you want to set.");
			}
		}

		private string GetBoardName(string moduleName, string positionIndex)
		{
			return "Board #" + positionIndex + "  ( " + moduleName + " )";
		}

		private void HideCommandDetails()
		{
			labelProbeState.Text = ""; labelProbeState.Visible = false;
			labelMinDelay.Text = ""; labelMinDelay.Visible = false;
			labelMaxDelay.Text = ""; labelMaxDelay.Visible = false;
			labelBoard.Text = ""; labelBoard.Visible = false;
			comboBoxBoard.Text = ""; comboBoxBoard.Visible = false;
			dataGridViewProbes.Rows.Clear(); dataGridViewProbes.Visible = false;
			textBoxMinDelay.Text = ""; textBoxMinDelay.Visible = false; labelSecondsMin.Visible = false;
			textBoxMaxDelay.Text = ""; textBoxMaxDelay.Visible = false; labelSecondsMax.Visible = false;
			textBoxTestName.Text = "";
			buttonHigh.Visible = false;
			buttonLow.Visible = false;
			buttonDontCare.Visible = false;
			buttonCircle.Visible = false;
			textBoxHexFile.Visible = false;

			textBoxTestName.Visible = false;
			labelNote.Visible = false;
			buttonUpdate.Visible = false;
			buttonUpdateButDisable.Visible = false;
		}

		private void PopulateCommandDetails(string preSetBoardName)
		{
			HideCommandDetails();

			if (testXmlDoc != null)
			{
				int id = GetSelectedTest();
				if (id >= 1)
				{
					XElement test = SearchElementById(stringStep, id);
					if (test != null)
					{
						string boardName = "";
						if (preSetBoardName.Length > 0)
						{
							boardName = preSetBoardName;
						}
						else
						{
							XElement b = test.Element("BoardId");
							if (b != null)
							{
								string b_id = b.Value;
								XElement module = SearchModuleByPosition(b_id);
								if (module != null)
								{
									string name = module.Element(stringModuleAlias).Value;
									boardName = GetBoardName(name, b_id);
								}
							}
						}
						labelNote.Text = "Note";
						string cmd = GetCurrentSelectedCommand();
						switch (cmd)
						{
							case stringCommand_VerifyTesterInput:
								labelBoard.Text = "Monitor outputs with:"; labelBoard.Visible = true;
								comboBoxBoard.Text = boardName;	comboBoxBoard.Visible = true;
								labelProbeState.Text = "Verify that signals must be:"; labelProbeState.Visible = true; dataGridViewProbes.Visible = true;
								break;
							case stringCommand_SetTesterOutput:
								labelBoard.Text = "Simulate inputs from:"; labelBoard.Visible = true;
								comboBoxBoard.Text = boardName;	comboBoxBoard.Visible = true;
								labelProbeState.Text = "Simulate signals:"; labelProbeState.Visible = true; dataGridViewProbes.Visible = true;
								break;
							case stringCommand_BarcodeScan:
							case stringCommand_BarcodeScan_Old:
								labelBoard.Text = "Read barcode from:"; labelBoard.Visible = true;
								comboBoxBoard.Text = boardName;	comboBoxBoard.Visible = true;
								labelProbeState.Text = "Trigger scanner:"; labelProbeState.Visible = true; dataGridViewProbes.Visible = true;
								break;
							case stringCommand_WaitRandom:
								labelBoard.Text = "Wait between:"; labelBoard.Visible = true;
								labelMinDelay.Text = "Minimum wait time"; labelMinDelay.Visible = true; textBoxMinDelay.Visible = true;
								labelSecondsMin.Text = "(Seconds)";	labelSecondsMin.Visible = true;
								labelMaxDelay.Text = "Maximum wait time"; labelMaxDelay.Visible = true; textBoxMaxDelay.Visible = true;  labelSecondsMax.Visible = true;
								break;
							case stringCommand_WaitForTesterInputs:
							case stringCommand_WaitOutput_Camera:
								labelBoard.Text = "Monitor outputs with:"; labelBoard.Visible = true;
								comboBoxBoard.Text = boardName;	comboBoxBoard.Visible = true;
								labelProbeState.Text = "After change occurs, the signals must be:"; labelProbeState.Visible = true;  dataGridViewProbes.Visible = true;
								labelMinDelay.Text = "Change must not occur within"; labelMinDelay.Visible = true; textBoxMinDelay.Visible = true;
								labelSecondsMin.Text = "(Seconds)";	labelSecondsMin.Visible = true;
								labelMaxDelay.Text = "Change must occur within"; labelMaxDelay.Visible = true; textBoxMaxDelay.Visible = true;  labelSecondsMax.Visible = true;
								break;
							case stringCommand_Wait:
								labelMinDelay.Text = "Wait"; labelMinDelay.Visible = true; textBoxMinDelay.Visible = true;
								labelSecondsMin.Text = "(Seconds)";	labelSecondsMin.Visible = true;
								break;
							case stringCommand_RepeatStart:
								labelMinDelay.Text = "Repeat"; labelMinDelay.Visible = true; textBoxMinDelay.Visible = true;
								labelSecondsMin.Text = "(Times)";	labelSecondsMin.Visible = true;
								break;
							case stringCommand_WaitUser:
							case stringCommand_DebugWaitUser:
							case stringCommand_RepeatEnd:
								break;
							case stringCommand_EraseRenesas:
							case stringCommand_EraseCypress:
								labelBoard.Text = "Erase program with:"; labelBoard.Visible = true;
								comboBoxBoard.Text = boardName;	comboBoxBoard.Visible = true;
								break;
							case stringCommand_FlashRenesas:
							case stringCommand_FlashMicrochip:
							case stringCommand_FlashCypress:
								labelBoard.Text = "Flash program with:"; labelBoard.Visible = true;
								comboBoxBoard.Text = boardName;	comboBoxBoard.Visible = true;
								labelProbeState.Text = "Target firmware file:"; labelProbeState.Visible = true;
								textBoxHexFile.Visible = true;
								labelNote.Text = "Model #:";
								break;
							case stringCommand_FlashInventek:
								labelBoard.Text = "Flash program with:"; labelBoard.Visible = true;
								comboBoxBoard.Text = boardName;	comboBoxBoard.Visible = true;
								labelProbeState.Text = "MAC log file:"; labelProbeState.Visible = true;
								textBoxHexFile.Visible = true;
								break;
							case stringCommand_ReadUart:
								labelBoard.Text = "Read Uart:"; labelBoard.Visible = true;
								comboBoxBoard.Text = boardName; comboBoxBoard.Visible = true;
								labelProbeState.Text = "Message string:"; labelProbeState.Visible = true;
								textBoxHexFile.Visible = true;
								break;
							case stringCommand_SearchUart:
								labelBoard.Text = "Search Uart:"; labelBoard.Visible = true;
								comboBoxBoard.Text = boardName;	comboBoxBoard.Visible = true;
								labelProbeState.Text = "Message string:"; labelProbeState.Visible = true;
								textBoxHexFile.Visible = true;
								break;
							case stringCommand_TransmitUart:
								labelBoard.Text = "Transmit Uart:"; labelBoard.Visible = true;
								comboBoxBoard.Text = boardName;	comboBoxBoard.Visible = true;
								labelProbeState.Text = "Message string:"; labelProbeState.Visible = true;
								textBoxHexFile.Visible = true;
								break;
							case stringCommand_SubScript:
								labelProbeState.Text = "Subscript:"; labelProbeState.Visible = true;
								textBoxHexFile.Visible = true;
								break;
							case stringCommand_WaitImage:
								labelProbeState.Text = "Image:"; labelProbeState.Visible = true;
								textBoxHexFile.Visible = true;
								/*labelMinDelay.Text = "Image ID:"; labelMinDelay.Visible = true; textBoxMinDelay.Visible = true;
								labelSecondsMin.Text = ""; labelSecondsMin.Visible = false;*/
								break;
							default:
								return;
						}
						textBoxTestName.Visible = true;
						labelNote.Visible = true;
						buttonUpdate.Visible = true;
						buttonUpdateButDisable.Visible = true;

						string ioDirection = GetCurrentSelectedCommandIoDirection();
						if (ioDirection == stringIoDirection_Input || ioDirection == stringIoDirection_Output)
						{
							foreach (XElement obj in testXmlDoc.Descendants(stringConnection))
							{
								string str = (string)obj.Element(stringConnectionAlias);
								string board = (string)obj.Element(stringBoardIndexOnTester);
								XElement module = SearchModuleByPosition(board);
								if (module == null)
								{
									continue;
								}
								string name = GetBoardName(module.Element(stringModuleAlias).Value, board);
								if (comboBoxBoard.Text != name)
								{
									continue;
								}
								string pinNumber = (string)obj.Element(stringPinNumberOnBoard);
								XElement pin = SearchPinById(board, pinNumber);
								if (pin != null)
								{
									string value = "";
									string direction = pin.Element(stringIoDirection).Value;
									if (direction != null && direction.Length > 0)
									{
										if (direction == ioDirection || ioDirection == stringIoDirection_Input)
										{
											var info = from element in test.Descendants("Probe")
													   where ((string)element.Element("Name") == str)
													   select element;
											if (info != null && info.Count() > 0)
											{
												foreach (XElement element in info.Distinct())
												{
													value = element.Element(stringValue).Value;
													break;
												}
											}
											string specialFlameBoardAdditionalChannels = "";
											XElement specialFlameBoardElement = pin.Element(stringSpecialFlameBoardAdditionalChannels);
											if (specialFlameBoardElement != null) specialFlameBoardAdditionalChannels = specialFlameBoardElement.Value;
											string specialPowerBoardAdditionalChannels = "";
											XElement specialPowerBoardElement = pin.Element(stringSpecialPowerBoardAdditionalChannels);
											if (specialPowerBoardElement != null) specialPowerBoardAdditionalChannels = specialPowerBoardElement.Value;
											string specialProgrammingBoardAdditionalChannels = "";
											XElement specialProgrammingBoardElement = pin.Element(stringSpecialProgrammingBoardAdditionalChannels);
											if (specialProgrammingBoardElement != null) specialProgrammingBoardAdditionalChannels = specialProgrammingBoardElement.Value;
											DataGridViewRow r = new DataGridViewRow();
											r.CreateCells(dataGridViewProbes);
											r.Cells[0].Value = str;
											r.Cells[1].Value = value + "                  ";
											r.Cells[2].Value = specialFlameBoardAdditionalChannels;
											r.Cells[3].Value = specialPowerBoardAdditionalChannels;
											r.Cells[4].Value = specialProgrammingBoardAdditionalChannels;
											dataGridViewProbes.Rows.Add(r);
										}
									}
								}
							}
						}
						switch (cmd)
						{
							case stringCommand_WaitRandom:
							case stringCommand_WaitForTesterInputs:
							case stringCommand_WaitOutput_Camera:
								XElement min = test.Element(stringMinValue);
								if (min != null) textBoxMinDelay.Text = min.Value;
								XElement max = test.Element(stringMaxValue);
								if (max != null) textBoxMaxDelay.Text = max.Value;
								break;
							case stringCommand_Wait:
								XElement wait = test.Element(stringValue);
								if (wait != null) textBoxMinDelay.Text = wait.Value;
								break;
							case stringCommand_RepeatStart:
								XElement repeat = test.Element(stringRepeatNumber);
								if (repeat != null) textBoxMinDelay.Text = repeat.Value;
								break;
							case stringCommand_WaitUser:
							case stringCommand_DebugWaitUser:
							case stringCommand_RepeatEnd:
								break;
							case stringCommand_EraseRenesas:
							case stringCommand_EraseCypress:
								break;
							case stringCommand_FlashRenesas:
							case stringCommand_FlashMicrochip:
							case stringCommand_FlashCypress:
								XElement hexFile = test.Element("FirmwareFile");
								if (hexFile != null) textBoxHexFile.Text = hexFile.Value;
								break;
							case stringCommand_FlashInventek:
								XElement macFile = test.Element("MacLogFile");
								if (macFile != null) textBoxHexFile.Text = macFile.Value;
								break;
							case stringCommand_ReadUart:
							case stringCommand_SearchUart:
							case stringCommand_TransmitUart:
								XElement stringId = test.Element("MessageString");
								if (stringId != null) textBoxHexFile.Text = stringId.Value;
								break;
							case stringCommand_SubScript:
								XElement subscript = test.Element("SubScript");
								if (subscript != null) textBoxMinDelay.Text = subscript.Value;
								break;
							case stringCommand_WaitImage:
								XElement image = test.Element("Image");
								if (image != null) textBoxMinDelay.Text = image.Value;
								break;
						}
						XElement note = test.Element(stringTestStepNote);
						if (note != null) textBoxTestName.Text = note.Value;
						DataGridShowButtons();
					}
				}
			}
		}

		private void comboBoxTestType_DropDown(object sender, EventArgs e)
		{
			comboBoxTestTypeBackup = comboBoxTestType.Text;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			newToolStripMenuItem_Click(sender, e);
		}

		private void comboBoxBoard_DropDownClosed(object sender, EventArgs e)
		{
			PopulateCommandDetails(comboBoxBoard.Text);
		}

		private int copiedStep = 0;

		private void buttonCopy_Click(object sender, EventArgs e)
		{
			int id = GetSelectedTest();
			if (id >= 1)
			{
				copiedStep = id;
			}
			else
			{
				copiedStep = 0;
			}
		}

		private void buttonPaste_Click(object sender, EventArgs e)
		{
			int id = GetSelectedTest();
			if (id >= 1 && copiedStep >= 1 && copiedStep != id)
			{
				XElement copied = SearchElementById(stringStep, copiedStep);
				XElement target = SearchElementById(stringStep, id);
				if (copied != null && target != null)
				{
					target.RemoveNodes();
					foreach (XElement ele in copied.Descendants())
					{
						XElement p = ele.Parent;
						if (p != null && p == copied)
						{
							target.Add(ele);
						}
					}
					LoadTestFile(id);
				}
			}
		}

		private DataGridViewRow GetSelectedRow()
		{
			if (dataGridViewProbes.Rows.Count >= 1)
			{
				int rows = dataGridViewProbes.SelectedCells.Count;
				if (rows == 1)
				{
					int row = dataGridViewProbes.SelectedCells[0].RowIndex;
					return dataGridViewProbes.Rows[row];
				}
			}
			return null;
		}

		private void dataGridViewProbes_SelectionChanged(object sender, EventArgs e)
		{
			buttonHigh.Visible = true;
			buttonLow.Visible = true;
			buttonDontCare.Visible = true;
		}

		private void buttonHigh_Click(object sender, EventArgs e)
		{
			DataGridViewRow r = GetSelectedRow();
			if (r != null) r.Cells[1].Value = "1";
		}

		private void buttonLow_Click(object sender, EventArgs e)
		{
			DataGridViewRow r = GetSelectedRow();
			if (r != null) r.Cells[1].Value = "0";
		}

		private void buttonDontCare_Click(object sender, EventArgs e)
		{
			DataGridViewRow r = GetSelectedRow();
			if (r != null) r.Cells[1].Value = "";
		}

		private void buttonCircle_Click(object sender, EventArgs e)
		{
			DataGridViewRow r = GetSelectedRow();
			if (r != null)
			{
				string str = ((string)(r.Cells[1].Value)).Trim();
				string str2 = ((string)(r.Cells[2].Value)).Trim();
				string str3 = ((string)(r.Cells[3].Value)).Trim();
				string str4 = ((string)(r.Cells[4].Value)).Trim();
				int current_data = 0;
				if (str.Length > 0) current_data = Convert.ToInt32(str);
				else current_data = 0;
				current_data++;
				int channel_num = 0;
				if (str2.Length > 0) channel_num = Convert.ToInt32(str2);
				else if (str3.Length > 0) channel_num = Convert.ToInt32(str3);
				else if (str4.Length > 0)
				{
					int num = Convert.ToInt32(str4);
					channel_num = (int)(Math.Pow(2, num) - 1);
				}
				if (channel_num == 0 || current_data > channel_num) current_data = 0;
				r.Cells[1].Value = current_data.ToString();
			}
		}

		private void DataGridShowButtons()
		{
			DataGridViewRow r = GetSelectedRow();
			if (r != null)
			{
				string str2 = ((string)(r.Cells[2].Value)).Trim();
				string str3 = ((string)(r.Cells[3].Value)).Trim();
				string str4 = ((string)(r.Cells[4].Value)).Trim();
				if (str2.Length > 0 || str3.Length > 0 || str4.Length > 0)
				{
					buttonCircle.Visible = true;
					buttonHigh.Visible = false;
					buttonLow.Visible = false;
					buttonDontCare.Visible = false;
				}
				else
				{
					buttonCircle.Visible = false;
					buttonHigh.Visible = true;
					buttonLow.Visible = true;
					buttonDontCare.Visible = true;
				}
			}
		}

		private void dataGridViewProbes_CellEnter(object sender, DataGridViewCellEventArgs e)
		{
			DataGridShowButtons();
		}

		private void dataGridViewProbes_Click(object sender, EventArgs e)
		{
			DataGridShowButtons();
		}

		private void boardSetupToolStripMenuItem_Click_1(object sender, EventArgs e)
		{
			tabControl1.SelectedTab = tabPageBoardSetup;
		}

		private void buttonInsertBoardBelow_Click(object sender, EventArgs e)
		{

		}

		private void buttonMoveBoardUp_Click(object sender, EventArgs e)
		{
			TreeNode node = treeViewBoards.SelectedNode;
			if (node != null)	// if a board is selected 
			{
				string currentBoardIndexOnTester = (string)node.Tag;
				if (currentBoardIndexOnTester != null && currentBoardIndexOnTester.Length > 0)	// a legitimate board to move
				{
					int currentPosition = Convert.ToInt32(currentBoardIndexOnTester);
					if (currentPosition > 0)	// allow to move up
					{
						string newBoardIndexOnTester = Convert.ToString(currentPosition - 1);
						XElement currentBoard = SearchBoardByPosition(currentBoardIndexOnTester);
						XElement exchangeBoard = SearchBoardByPosition(newBoardIndexOnTester);
						if (currentBoard != null && exchangeBoard != null)	// current board and exchange board found
						{
							// change position
							currentBoard.Element(stringBoardIndexOnTester).Value = newBoardIndexOnTester;
							exchangeBoard.Element(stringBoardIndexOnTester).Value = currentBoardIndexOnTester;
							// change all references to these two boards' position
							MessageBox.Show("Need to evaluate effect of board moving up.");
							// update display
							ShowTesterConnections("", "");
							MessageBox.Show("Need to evaluate effect of test step display.");
							return;
						}
					}
				}
			}
			MessageBox.Show("Please select the board which you want to move up.");
		}

		private void buttonMoveBoardDown_Click(object sender, EventArgs e)
		{

		}

		private void buttonUpdateButDisable_Click(object sender, EventArgs e)
		{
			int id = GetSelectedTest();
			if (id >= 1)
			{
				UpdateTestStep(id, true);
			}
			return;
		}

		private void buttonDebug_Click(object sender, EventArgs e)
		{
			InsertTestStep(true);
			int id = GetSelectedTest();
			if (id >= 1)
			{
				XElement test = SearchElementById(stringStep, id);
				if (test != null)
				{
					test.RemoveNodes();
					XElement cmd = new XElement(stringCommandCodeName, stringCommand_DebugWaitUser);
					test.Add(cmd);
					XElement alias = new XElement(stringCommandName, GetCommandAlias(stringCommand_DebugWaitUser));
					test.Add(alias);
					XElement note = new XElement(stringTestStepNote, "");
					test.Add(note);
					LoadTestFile(id);
				}
			}
		}


		/*
					TreeNode parent = node.Parent;
					if (parent != null)
					{
						string currentBoardIndexOnTester = (string)parent.Tag;
						string pinNumberOnBoard = (string)node.Tag;
						XElement obj = SearchForProbeElement("", currentBoardIndexOnTester, pinNumberOnBoard);
						if (obj == null)
						{
							XElement pin = SearchPinById(currentBoardIndexOnTester, pinNumberOnBoard);
							if (pin != null)
							{
								string ioDirection = (string)pin.Element(stringIoDirection).Value;
								if (ioDirection == "-")
								{
									MessageBox.Show("This is a reference pin without a specific function.");
									return;
								}
							}
							DialogResult result = dlg.ShowDialog();
							switch (result)
							{
								case System.Windows.Forms.DialogResult.OK:
									string newName = dlg.textBoxInput.Text.Trim();
									if (newName.Length > 0)
									{
										// check if replacedByNewAlias is currently used, proceed if only not
										if (SearchForProbeElement(newName, "", "") == null)
										{
											AddProbe(newName, currentBoardIndexOnTester, pinNumberOnBoard);
											ShowTesterConnections(currentBoardIndexOnTester, pinNumberOnBoard);
										}
										else
										{
											MessageBox.Show("Probe name \"" + newName + "\" is already used.");
										}
									}
									break;
							}
						}
					}
*/

	}
}
