﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Alphora.Dataphor.DAE.Runtime.Data;
using Alphora.Dataphor.DAE.Client;

namespace Alphora.Dataphor.Dataphoria
{
	public partial class DebugProcessesView : UserControl
	{
		public DebugProcessesView()
		{
			InitializeComponent();
		}

		private IDataphoria FDataphoria;
		public IDataphoria Dataphoria
		{
			get { return FDataphoria; }
			set
			{
				if (FDataphoria != value)
				{
					if (FDataphoria != null)
					{
						FDataphoria.Disconnected -= new EventHandler(FDataphoria_Disconnected);
						FDataphoria.Connected -= new EventHandler(FDataphoria_Connected);
						FDataphoria.Debugger.PropertyChanged -= Debugger_PropertyChanged;
						FDataphoria.Debugger.SessionAttached -= new EventHandler(Debugger_AttachmentChanged);
						FDataphoria.Debugger.SessionDetached -= new EventHandler(Debugger_AttachmentChanged);
						FDataphoria.Debugger.ProcessAttached -= new EventHandler(Debugger_AttachmentChanged);
						FDataphoria.Debugger.ProcessDetached -= new EventHandler(Debugger_AttachmentChanged);
					}
					FDataphoria = value;
					if (FDataphoria != null)
					{
						FDataphoria.Disconnected += new EventHandler(FDataphoria_Disconnected);
						FDataphoria.Connected += new EventHandler(FDataphoria_Connected);
						FDataphoria.Debugger.PropertyChanged += Debugger_PropertyChanged;
						FDataphoria.Debugger.SessionAttached += new EventHandler(Debugger_AttachmentChanged);
						FDataphoria.Debugger.SessionDetached += new EventHandler(Debugger_AttachmentChanged);
						FDataphoria.Debugger.ProcessAttached += new EventHandler(Debugger_AttachmentChanged);
						FDataphoria.Debugger.ProcessDetached += new EventHandler(Debugger_AttachmentChanged);
						if (FDataphoria.IsConnected)
						{
							FDataphoria_Connected(this, EventArgs.Empty);
							UpdateDataView();
						}
					}
				}
			}
		}

		private void Debugger_AttachmentChanged(object sender, EventArgs e)
		{
			RefreshDataView();
		}

		private void Debugger_PropertyChanged(object ASender, string[] APropertyNames)
		{
			try
			{
				if (Array.Exists<string>(APropertyNames, (string AItem) => { return AItem == "IsStarted" || AItem == "IsPaused" || AItem == "SelectedProcessID"; }))
					UpdateDataView();
			}
			catch (Exception LException)
			{
				FDataphoria.Warnings.AppendError(null, LException, false);
			}
		}

		private void FDataphoria_Disconnected(object sender, EventArgs e)
		{
			FDebugProcessDataView.Close();
			FDebugProcessDataView.Session = null;
			DeinitializeParamGroup();
		}
		
		private void FDataphoria_Connected(object sender, EventArgs e)
		{
			FDebugProcessDataView.Session = FDataphoria.DataSession;
			InitializeParamGroup();
		}

		private void FDetachButton_Click(object sender, EventArgs e)
		{
			if (FDebugProcessDataView.Active && !FDebugProcessDataView.IsEmpty())
				FDataphoria.Debugger.DetachProcess(FDebugProcessDataView["ID"].AsInt32);
		}

		private void FRefreshButton_Click(object sender, EventArgs e)
		{
			RefreshDataView();
		}
		
		private void RefreshDataView()
		{
			if (FDebugProcessDataView.Active)
				FDebugProcessDataView.Refresh();
		}

		private void UpdateDataView()
		{
			if (Dataphoria.Debugger.IsStarted)
			{
				// Save old postion
				Row LOld = null;
				if (FDebugProcessDataView.Active && !FDebugProcessDataView.IsEmpty())
					LOld = FDebugProcessDataView.ActiveRow;
					
				// Update the selected process
				FSelectedProcessIDParam.Value = FDataphoria.Debugger.SelectedProcessID;
				
				// Open the DataView
				FDebugProcessDataView.Open();

				// Attempt to seek to old position
				if (LOld != null)
					FDebugProcessDataView.Refresh(LOld);
			}
			else
				FDebugProcessDataView.Close();
		}

		private void FDebugProcessDataView_DataChanged(object sender, EventArgs e)
		{
			UpdateButtonsEnabled();
		}

		private void UpdateButtonsEnabled()
		{
			FRefreshButton.Enabled = FDebugProcessDataView.Active;
			FRefreshContextMenuItem.Enabled = FRefreshButton.Enabled;
			var LHasRow = FDebugProcessDataView.Active && !FDebugProcessDataView.IsEmpty();
			FDetachButton.Enabled = LHasRow;
			FDetachContextMenuItem.Enabled = LHasRow;
			FSelectButton.Enabled = LHasRow;
			FSelectContextMenuItem.Enabled = LHasRow;
		}

		private void FSelectButton_Click(object sender, EventArgs e)
		{
			if (FDebugProcessDataView.Active && !FDebugProcessDataView.IsEmpty())
				FDataphoria.Debugger.SelectedProcessID = FDebugProcessDataView["Process_ID"].AsInt32;
		}

		private void InitializeParamGroup()
		{
			if (FGroup == null)
			{
				FGroup = new DataSetParamGroup();
				FSelectedProcessIDParam = new DataSetParam() { Name = "ASelectedProcessID", DataType = FDataphoria.UtilityProcess.DataTypes.SystemInteger };
				FGroup.Params.Add(FSelectedProcessIDParam);
				FDebugProcessDataView.ParamGroups.Add(FGroup);
			}
		}

		private void DeinitializeParamGroup()
		{
			if (FGroup != null)
			{
				FDebugProcessDataView.ParamGroups.Remove(FGroup);
				FGroup.Dispose();
				FGroup = null;
			}
		}

		private DataSetParam FSelectedProcessIDParam;
		private DataSetParamGroup FGroup;
	}
}