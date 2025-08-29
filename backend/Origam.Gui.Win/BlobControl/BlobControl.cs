#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using Origam.Schema;
using Origam.DA;
using Origam.UI;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;

namespace Origam.Gui.Win
{
	/// <summary>
	/// Summary description for BlobControl.
	/// </summary>
	public class BlobControl : BaseCaptionControl
	{
		private System.Windows.Forms.ImageList imageList1;
		private Origam.Gui.Win.AsDropDown.NoKeyUpTextBox txtEdit;
		private System.Windows.Forms.Button btnDropDown;
		private System.ComponentModel.IContainer components;
		private IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
		private ToolStripMenuItem mnuUpload;
        private ToolStripMenuItem mnuDownload;
        private ToolStripMenuItem mnuPreview;
        private ToolStripMenuItem mnuDelete;

		private Hashtable _openFiles = new Hashtable();

		public BlobControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

            mnuUpload = new ToolStripMenuItem(ResourceUtils.GetString("MenuUpload"));
			mnuUpload.Click += new EventHandler(mnuUpload_Click);
            mnuUpload.Image = Origam.Workbench.Images.Open;

            mnuDownload = new ToolStripMenuItem(ResourceUtils.GetString("MenuDownload"));
			mnuDownload.Click += new EventHandler(mnuDownload_Click);
            mnuDownload.Image = Origam.Workbench.Images.Save;

            mnuPreview = new ToolStripMenuItem(ResourceUtils.GetString("MenuPreview"));
			mnuPreview.Click += new EventHandler(mnuPreview_Click);
            mnuPreview.Image = Origam.Workbench.Images.Preview;

            mnuDelete = new ToolStripMenuItem(ResourceUtils.GetString("MenuDelete"));
            mnuDelete.Click += new EventHandler(mnuDelete_Click);
            mnuDelete.Image = Origam.Workbench.Images.Delete;

			txtEdit.ContextMenuStrip = new ContextMenuStrip();
            txtEdit.ContextMenuStrip.Items.AddRange(new ToolStripMenuItem[] { mnuUpload, mnuDownload, mnuPreview, mnuDelete });
            txtEdit.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
			txtEdit.MouseDown += new MouseEventHandler(txtEdit_MouseDown);
			txtEdit.KeyDown += new KeyEventHandler(txtEdit_KeyDown);

			// Color Scheme
			txtEdit.ForeColor = OrigamColorScheme.LinkColor;
			btnDropDown.BackColor = OrigamColorScheme.ButtonBackColor;
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
//				mnuUpload.Click -= new EventHandler(mnuUpload_Click);
//				mnuDownload.Click -= new EventHandler(mnuDownload_Click);
//				mnuPreview.Click -= new EventHandler(mnuPreview_Click);

				if(txtEdit.ContextMenuStrip != null)
				{
                    txtEdit.ContextMenuStrip.Opening -= ContextMenuStrip_Opening;
					ContextMenuStrip cm = this.txtEdit.ContextMenuStrip;
					cm.Dispose();
				}

				txtEdit.MouseDown -= new MouseEventHandler(txtEdit_MouseDown);
				txtEdit.KeyDown -= new KeyEventHandler(txtEdit_KeyDown);
				

				_persistence = null;

				_openFiles.Clear();

				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			AsDataGrid grid = this.Parent as AsDataGrid;
			if(grid != null)
			{
				grid.OnControlMouseWheel(e);
			}
			else
			{
				base.OnMouseWheel (e);
			}
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BlobControl));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.txtEdit = new Origam.Gui.Win.BaseDropDownControl.NoKeyUpTextBox();
            this.btnDropDown = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Magenta;
            this.imageList1.Images.SetKeyName(0, "");
            this.imageList1.Images.SetKeyName(1, "");
            // 
            // txtEdit
            // 
            this.txtEdit.CustomFormat = null;
            this.txtEdit.DataType = typeof(string);
            this.txtEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtEdit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.txtEdit.ForeColor = System.Drawing.Color.Black;
            this.txtEdit.IgnoreCursorDown = false;
            this.txtEdit.Location = new System.Drawing.Point(0, 0);
            this.txtEdit.Name = "txtEdit";
            this.txtEdit.NoKeyUp = false;
            this.txtEdit.Size = new System.Drawing.Size(152, 23);
            this.txtEdit.TabIndex = 4;
            this.txtEdit.Value = "";
            this.txtEdit.MouseEnter += new System.EventHandler(this.txtEdit_MouseEnter);
            this.txtEdit.MouseHover += new System.EventHandler(this.txtEdit_MouseHover);
            this.txtEdit.MouseMove += new System.Windows.Forms.MouseEventHandler(this.txtEdit_MouseMove);
            // 
            // btnDropDown
            // 
            this.btnDropDown.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(203)))), ((int)(((byte)(111)))));
            this.btnDropDown.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnDropDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDropDown.ForeColor = System.Drawing.Color.White;
            this.btnDropDown.ImageIndex = 0;
            this.btnDropDown.ImageList = this.imageList1;
            this.btnDropDown.Location = new System.Drawing.Point(152, 0);
            this.btnDropDown.Name = "btnDropDown";
            this.btnDropDown.Size = new System.Drawing.Size(16, 20);
            this.btnDropDown.TabIndex = 5;
            this.btnDropDown.TabStop = false;
            this.btnDropDown.UseVisualStyleBackColor = false;
            this.btnDropDown.Click += new System.EventHandler(this.btnDropDown_Click);
            // 
            // BlobControl
            // 
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.txtEdit);
            this.Controls.Add(this.btnDropDown);
            this.Name = "BlobControl";
            this.Size = new System.Drawing.Size(168, 20);
            this.Resize += new System.EventHandler(this.BlobControl_Resize);
            this.ResumeLayout(false);
            this.PerformLayout();
		}
		#endregion

		#region Properties
		public override string DefaultBindableProperty
		{
			get
			{
				return "FileName";
			}
		}

		private bool _noKeyUp = false;
		public bool NoKeyUp
		{
			get
			{
				return _noKeyUp;
			}
			set
			{
				_noKeyUp = value;
				this.txtEdit.NoKeyUp = value;
			}
		}

		public Control EditControl
		{
			get
			{
				return txtEdit;
			}
		}

		private bool _readOnly = false;
		public bool ReadOnly
		{
			get
			{
				return _readOnly;
			}
			set
			{
				_readOnly = value;

				if(_readOnly)
				{
					this.txtEdit.ReadOnly = true;
					//this.btnDropDown.Visible = false;
				}
				else
				{
					this.txtEdit.ReadOnly = false;
					//this.btnDropDown.Visible = true;
				}
			}
		}

		string _fileName;
		public string FileName
		{
			get
			{
				return _fileName;
			}
			set
			{
				if(_fileName == value)
                {
                    return;
                }

                if (value == "")
                {
                    value = null;
                }

                _fileName = value;

				this.txtEdit.Value = value;

				OnFileNameChanged(EventArgs.Empty);
			}
		}

		string _originalPathMember;
		[Category("Data Members")]
		public string OriginalPathMember
		{
			get
			{
				return _originalPathMember;
			}
			set
			{
				_originalPathMember = value;
			}
		}

		string _fileSizeMember;
		[Category("Data Members")]
		public string FileSizeMember
		{
			get
			{
				return _fileSizeMember;
			}
			set
			{
				_fileSizeMember = value;
			}
		}

		string _blobMember;
		[Category("Data Members")]
		public string BlobMember
		{
			get
			{
				return _blobMember;
			}
			set
			{
				_blobMember = value;
			}
		}

		string _thumbnailMember;
		[Category("Data Members")]
		public string ThumbnailMember
		{
			get
			{
				return _thumbnailMember;
			}
			set
			{
				_thumbnailMember = value;
			}
		}

		string _dateCreatedMember;
		[Category("Data Members")]
		public string DateCreatedMember
		{
			get
			{
				return _dateCreatedMember;
			}
			set
			{
				_dateCreatedMember = value;
			}
		}

		string _dateLastModifiedMember;
		[Category("Data Members")]
		public string DateLastModifiedMember
		{
			get
			{
				return _dateLastModifiedMember;
			}
			set
			{
				_dateLastModifiedMember = value;
			}
		}

		string _authorMember;
		[Category("Data Members")]
		public string AuthorMember
		{
			get
			{
				return _authorMember;
			}
			set
			{
				_authorMember = value;
			}
		}

		string _remarkMember;
		[Category("Data Members")]
		public string RemarkMember
		{
			get
			{
				return _remarkMember;
			}
			set
			{
				_remarkMember = value;
			}
		}

		string _compressionStateMember;
		[Category("Data Members")]
		public string CompressionStateMember
		{
			get
			{
				return _compressionStateMember;
			}
			set
			{
				_compressionStateMember = value;
			}
		}

		BlobStorageType _storageType;
		public BlobStorageType StorageType
		{
			get
			{
				return _storageType;
			}
			set
			{
				_storageType = value;
			}
		}

		bool _displayStorageTypeSelection;
		[Category("Blob Settings")]
		public bool DisplayStorageTypeSelection
		{
			get
			{
				return _displayStorageTypeSelection;
			}
			set
			{
				_displayStorageTypeSelection = value;
			}
		}

		private Guid _blobLookupId;
		[Browsable(false)]
		public Guid BlobLookupId
		{
			get
			{
				return _blobLookupId;
			}
			set
			{
				_blobLookupId = value;
			}
		}
		
		[TypeConverter(typeof(DataLookupConverter))]
		[Category("Blob Settings")]
		public IDataLookup BlobLookup
		{
			get
			{
				return (IDataLookup)_persistence.SchemaProvider.RetrieveInstance(typeof(ISchemaItem), new ModelElementKey(this.BlobLookupId));
			}
			set
			{
				this.BlobLookupId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		private Guid _thumbnailWidthConstantId;
		[Browsable(false)]
		public Guid ThumbnailWidthConstantId
		{
			get
			{
				return _thumbnailWidthConstantId;
			}
			set
			{
				_thumbnailWidthConstantId = value;
			}
		}
		
		[TypeConverter(typeof(DataConstantConverter))]
		[Category("Blob Settings")]
		public DataConstant ThumbnailWidthConstant
		{
			get
			{
				return (DataConstant)_persistence.SchemaProvider.RetrieveInstance(typeof(DataConstant), new ModelElementKey(this.ThumbnailWidthConstantId));
			}
			set
			{
				this.ThumbnailWidthConstantId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		private Guid _thumbnailHeightConstantId;
		[Browsable(false)]
		public Guid ThumbnailHeightConstantId
		{
			get
			{
				return _thumbnailHeightConstantId;
			}
			set
			{
				_thumbnailHeightConstantId = value;
			}
		}
		
		[TypeConverter(typeof(DataConstantConverter))]
		[Category("Blob Settings")]
		public DataConstant ThumbnailHeightConstant
		{
			get
			{
				return (DataConstant)_persistence.SchemaProvider.RetrieveInstance(typeof(DataConstant), new ModelElementKey(this.ThumbnailHeightConstantId));
			}
			set
			{
				this.ThumbnailHeightConstantId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		[Browsable(false)]
		private Guid _storageTypeDefaultConstantId;
		[Browsable(false)]
		public Guid StorageTypeDefaultConstantId
		{
			get
			{
				return _storageTypeDefaultConstantId;
			}
			set
			{
				_storageTypeDefaultConstantId = value;
			}
		}
		
		[TypeConverter(typeof(DataConstantConverter))]
		[Category("Blob Settings")]
		public DataConstant StorageTypeDefaultConstant
		{
			get
			{
				return (DataConstant)_persistence.SchemaProvider.RetrieveInstance(typeof(DataConstant), new ModelElementKey(this.StorageTypeDefaultConstantId));
			}
			set
			{
				this.StorageTypeDefaultConstantId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		[Browsable(false)]
		private Guid _defaultCompressionConstantId;
		[Browsable(false)]
		public Guid DefaultCompressionConstantId
		{
			get
			{
				return _defaultCompressionConstantId;
			}
			set
			{
				_defaultCompressionConstantId = value;
			}
		}
		
		[TypeConverter(typeof(DataConstantConverter))]
		[Category("Blob Settings")]
		public DataConstant DefaultCompressionConstant
		{
			get
			{
				return (DataConstant)_persistence.SchemaProvider.RetrieveInstance(typeof(DataConstant), new ModelElementKey(this.DefaultCompressionConstantId));
			}
			set
			{
				this.DefaultCompressionConstantId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		
		#endregion

		#region Methods
		#endregion

		#region Private Methods
		private DataRow CurrentRow
		{
			get
			{
				return DataBindingTools.CurrentRow(this, DefaultBindableProperty);
			}
		}

		private bool IsControlPressed
		{
			get
			{
				return (Control.ModifierKeys & Keys.Control) == Keys.Control;
			}
		}

		private bool IsCompressed
		{
			get
			{
				CheckCurrentRow();

				bool compressed = false;
				if(CheckMember("CompressionStateMember", false))
				{
					compressed = (bool)CurrentRow[this.CompressionStateMember];
				}

				return compressed;
			}
		}

		private bool ShouldCompress
		{
			get
			{
				CheckCurrentRow();

				if(CheckMember("CompressionStateMember", false))
				{
					IParameterService param = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

					bool compress = false;
					if(DefaultCompressionConstant != null)
					{
						compress = (bool)param.GetParameterValue(DefaultCompressionConstantId, OrigamDataType.Boolean);
					}

					return compress;
				}

                return false;
            }
		}

		private bool CanPreview()
		{
			return this.FileName != null & this.FileName != String.Empty;
		}

		private bool CheckMember(string member, bool throwExceptions)
		{
			object val = Reflector.GetValue(this.GetType(), this, member);

			if(val == null || val.Equals(String.Empty))
			{
				if(throwExceptions)
				{
					throw new NullReferenceException(member + " not set.");
				}

                return false;
            }

			return true;
		}

		private void CheckCurrentRow()
		{
			if(CurrentRow == null)
            {
                throw new NullReferenceException(ResourceUtils.GetString("ErrorHandleBlob"));
            }
        }

		private void Upload()
		{
			try
			{
				IParameterService param = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

				CheckCurrentRow();
				CheckMember("BlobMember", true);

				OpenFileDialog _dlgOpen = new OpenFileDialog();

				if (_dlgOpen.ShowDialog()==DialogResult.OK)
				{
					string path = _dlgOpen.FileName;

					DataRow row = CurrentRow;
					row.BeginEdit();

					OnValueChangingByUser(EventArgs.Empty);
				
					if(CheckMember("OriginalPathMember", false))
					{
						row[this.OriginalPathMember] = path;
					}

					if(CheckMember("DateCreatedMember", false))
					{
						row[this.DateCreatedMember] = File.GetCreationTime(path);
					}
				
					if(CheckMember("DateLastModifiedMember", false))
					{
						row[this.DateLastModifiedMember] = File.GetLastWriteTime(path);
					}

					if(CheckMember("CompressionStateMember", false))
					{
						row[this.CompressionStateMember] = this.ShouldCompress;
					}

					ByteArrayConverter.SaveToDataSet(path, row, this.BlobMember, this.ShouldCompress);

					if(CheckMember("FileSizeMember", false))
					{
						row[this.FileSizeMember] = ((byte[])row[this.BlobMember]).LongLength;
					}

					if(CheckMember("ThumbnailMember", false))
					{
						Image img = null;

						try
						{
							img = Image.FromFile(path);
						}
						catch
						{
							row[this.ThumbnailMember] = DBNull.Value;
						}
						
						if(img != null)
						{
							try
							{
								int width = (int)param.GetParameterValue(this.ThumbnailWidthConstantId, OrigamDataType.Integer);
								int height = (int)param.GetParameterValue(this.ThumbnailHeightConstantId, OrigamDataType.Integer);

								Image.GetThumbnailImageAbort myCallback = new Image.GetThumbnailImageAbort(ThumbnailCallback);

								using(Image thumbnail = ImageResizer.FixedSize(img, width, height))
								{
									MemoryStream ms = new MemoryStream();

									try
									{
										thumbnail.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
										byte[] byteArray = ms.GetBuffer();
										row[this.ThumbnailMember] = byteArray;
									}
									finally
									{
										if(ms != null)
                                        {
                                            ms.Close();
                                        }
                                    }
								}
							}
							finally
							{
								if(img != null)
                                {
                                    img.Dispose();
                                }
                            }
						}
					}

					try
					{
						if(! (this.Parent is DataGrid))
						{
							CurrencyManager c = this.BindingContext[row.Table.DataSet, FormGenerator.DataMemberFromTable(row.Table)] as CurrencyManager;
							c.Refresh();
						}
					}
					catch
					{
					}

					// we do this in the end, because CurrencyManager.Refresh would revert the value back
					this.FileName = System.IO.Path.GetFileName(path);
				}
			}			
			catch(Exception ex)
			{
				AsMessageBox.ShowError(this, ex.Message, ResourceUtils.GetString("ErrorSaveFile"), ex);
			}
		}

		private void Download()
		{
			try
			{
				CheckCurrentRow();

				DataRow row = CurrentRow;

				SaveFileDialog dlgSave = new SaveFileDialog();
				dlgSave.OverwritePrompt = true;
				dlgSave.FileName = this.FileName;

				if (dlgSave.ShowDialog()==DialogResult.OK)
				{
					Download(dlgSave.FileName);
				}
			}
			catch(Exception ex)
			{
				AsMessageBox.ShowError(this, ex.Message, ResourceUtils.GetString("ErrorLoadFile"), ex);
			}
		}

		private bool Download(string path)
		{
			try
			{
				CheckCurrentRow();

				DataRow row = CurrentRow;
				
				if(BlobLookup != null)
				{
					IDataLookupService lookupService = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;

					object result = lookupService.GetDisplayText(this.BlobLookupId, row["Id"], false, false, null);
					byte[] bytes;

					if(result == null)
					{
						throw new Exception("Data source did not return any data.");
					}
					else if(result is byte[])
					{
						bytes = (byte[])result;
					}
					else
					{
						throw new InvalidCastException(ResourceUtils.GetString("ErrorNotBlob"));
					}
					
					ByteArrayConverter.ByteArrayToFile(path, bytes, IsCompressed);
				}
				else
				{
					CheckMember("BlobMember", true);

					if(row[this.BlobMember] == DBNull.Value)
					{
						throw new Exception(ResourceUtils.GetString("ErrorRecordEmpty"));
					}

                    ByteArrayConverter.SaveFromDataSet(path, row, BlobMember, IsCompressed);
                }

				return true;
			}
			catch (Exception ex)
			{
				AsMessageBox.ShowError(this, ex.Message, ResourceUtils.GetString("ErrorFileSave"), ex);
				return false;
			}
		}

		private void ExecutePreview()
		{
			string filePath = "";

			try
			{
				filePath = System.IO.Path.GetTempFileName();
				filePath = Path.ChangeExtension(filePath, Path.GetExtension(this.FileName));
			}
			catch(Exception ex)
			{
				AsMessageBox.ShowError(this.FindForm(), ResourceUtils.GetString("ErrorTmpName"), ResourceUtils.GetString("ErrorShowPreviewTitle"), ex);
			}

			try
			{
				if(Download(filePath))
				{
					System.Diagnostics.Process process = System.Diagnostics.Process.Start(filePath);
					process.EnableRaisingEvents = true;

					_openFiles.Add(process, filePath);
					process.Exited += new EventHandler(process_Exited);
				}		
			}
			catch(Exception ex)
			{
				AsMessageBox.ShowError(this.FindForm(), ResourceUtils.GetString("ErrorExecuteFile", filePath), ResourceUtils.GetString("ErrorShowPreviewTitle"), ex);
			}
		}

		#endregion

		#region Events
		public event System.EventHandler fileNameChanged;
		protected virtual void OnFileNameChanged(EventArgs e)
		{
			if (this.fileNameChanged != null)
			{
				this.fileNameChanged(this, e);
			}
		}

		public event System.EventHandler ValueChangingByUser;
		protected virtual void OnValueChangingByUser(EventArgs e)
		{
			if (this.ValueChangingByUser != null)
			{
				this.ValueChangingByUser(this, e);
			}
		}
		#endregion

		#region Event Handlers
		private void btnDropDown_Click(object sender, System.EventArgs e)
		{
			txtEdit.ContextMenuStrip.Show(txtEdit, new Point(txtEdit.Left, txtEdit.Bottom));
		}

		private void mnuUpload_Click(object sender, EventArgs e)
		{
			Upload();
		}

		private void mnuDownload_Click(object sender, EventArgs e)
		{
			Download();
		}

		private void mnuPreview_Click(object sender, EventArgs e)
		{
			if(CanPreview())
			{
				ExecutePreview();
			}
		}

        private void mnuDelete_Click(object sender, EventArgs e)
        {
            DataRow row = CurrentRow;
            row.EndEdit();
            if (CheckMember("OriginalPathMember", false))
            {
                SetNull(row, this.OriginalPathMember);
            }
            if (CheckMember("DateCreatedMember", false))
            {
                SetNull(row, this.DateCreatedMember);
            }
            if (CheckMember("DateLastModifiedMember", false))
            {
                SetNull(row, this.DateLastModifiedMember);
            }
            if (CheckMember("CompressionStateMember", false))
            {
                SetNull(row, this.CompressionStateMember);
            }
            if (CheckMember("FileSizeMember", false))
            {
                SetNull(row, this.FileSizeMember);
            }
            if (CheckMember("ThumbnailMember", false))
            {
                SetNull(row, this.ThumbnailMember);
            }
            this.FileName = null;
        }

        private static void SetNull(DataRow row, string member)
        {
            if (CheckNullable(row, member))
            {
                row[member] = DBNull.Value;
            }
        }

        private static bool CheckNullable(DataRow row, string member)
        {
            return row.Table.Columns[member].AllowDBNull;
        }
        
        private void txtEdit_MouseDown(object sender, MouseEventArgs e)
		{
			if(CanPreview() & IsControlPressed)
			{
				ExecutePreview();
			}
		}

        void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            bool canPreview = this.CanPreview();
			mnuUpload.Visible = !this.ReadOnly;
            mnuPreview.Enabled = canPreview;
            mnuDownload.Enabled = canPreview;
            mnuDelete.Enabled = canPreview;
		}

		private void process_Exited(object sender, EventArgs e)
		{
			System.Diagnostics.Process process = sender as System.Diagnostics.Process;
			process.Exited -= new EventHandler(process_Exited);

			string filePath = (string)_openFiles[process];

			if(filePath != null)
			{
				try
				{
					System.IO.File.Delete(filePath);
				}
				finally
				{
					_openFiles.Remove(process);
				}
			}
		}

		private void txtEdit_MouseHover(object sender, EventArgs e)
		{
			this.OnMouseHover(e);
		}

		private void txtEdit_MouseEnter(object sender, EventArgs e)
		{
			this.OnMouseEnter(e);
		}

		private void txtEdit_MouseMove(object sender, MouseEventArgs e)
		{
			if(CanPreview() & IsControlPressed)
			{
				txtEdit.Cursor = Cursors.Hand;
			}
			else
			{
				txtEdit.Cursor = Cursors.IBeam;
			}

			this.OnMouseMove(e);
		}

		private void txtEdit_KeyDown(object sender, KeyEventArgs e)
		{
			if(e.KeyCode != Keys.Left & e.KeyCode != Keys.Right & e.KeyCode != Keys.Up & e.KeyCode != Keys.Down & e.KeyCode != Keys.Tab & e.KeyCode != Keys.Escape & e.KeyCode != Keys.ControlKey & e.KeyCode != Keys.Alt & e.KeyCode != Keys.ShiftKey)
			{
				OnValueChangingByUser(EventArgs.Empty);
			}
		}

		private void BlobControl_Resize(object sender, EventArgs e)
		{
			this.Height = txtEdit.Height;
		}

		private bool ThumbnailCallback()
		{
			return true;
		}
		#endregion
	}
}
