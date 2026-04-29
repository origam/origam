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
using Origam.UI;
using WeifenLuo.WinFormsUI.Docking;

namespace Origam.Workbench;

public class AbstractViewContent : AbstractBaseViewContent, IViewContent
{
    string _untitledName = String.Empty;
    string _titleName = null;
    Guid displayedItemId;

    bool _isDirty = false;
    bool _isViewOnly = false;
    bool _canRefresh = false;

    public virtual bool CanRefreshContent
    {
        get { return _canRefresh; }
        set { _canRefresh = value; }
    }
    public virtual string UntitledName
    {
        get { return _untitledName; }
        set { _untitledName = value; }
    }

    public virtual string TitleName
    {
        get { return IsUntitled ? _untitledName : _titleName; }
        set
        {
            _titleName = value;
            OnTitleNameChanged(e: EventArgs.Empty);
        }
    }

    private string _statusText;
    public string StatusText
    {
        get { return _statusText; }
        set
        {
            _statusText = value;
            OnStatusTextChanged(e: EventArgs.Empty);
        }
    }
    public event EventHandler StatusTextChanged;

    void OnStatusTextChanged(EventArgs e)
    {
        if (StatusTextChanged != null)
        {
            StatusTextChanged(sender: this, e: e);
        }
    }

    public virtual Guid DisplayedItemId
    {
        get => displayedItemId;
        set
        {
            displayedItemId = value;
            OnFileNameChanged(e: EventArgs.Empty);
        }
    }
    public virtual string HelpTopic
    {
        get { return ""; }
    }

    public virtual bool CreateAsSubViewContent
    {
        get { return false; }
    }

    public AbstractViewContent() { }

    public virtual void RefreshContent() { }

    public AbstractViewContent(string titleName)
    {
        _titleName = titleName;
    }

    public AbstractViewContent(string titleName, string fileName)
    {
        _titleName = titleName;
        displayedItemId = Guid.Parse(input: fileName);
    }

    public bool IsUntitled
    {
        get { return _titleName == null; }
    }

    public virtual string Test()
    {
        return "";
    }

    public virtual bool IsDirty
    {
        get { return _isDirty; }
        set
        {
            if (_isDirty != value)
            {
                _isDirty = value;
                OnDirtyChanged(e: EventArgs.Empty);
            }
        }
    }

    private bool _isReadOnly = false;
    public virtual bool IsReadOnly
    {
        get { return _isReadOnly; }
        set { _isReadOnly = value; }
    }

    public virtual bool IsViewOnly
    {
        get
        {
            if (IsReadOnly)
            {
                return true;
            }

            return _isViewOnly;
        }
        set { _isViewOnly = value; }
    }

    public virtual void SaveObject()
    {
        if (IsDirty)
        {
            throw new System.NotImplementedException();
        }
    }

    public object LoadedObject { get; private set; }

    protected virtual void ViewSpecificLoad(object objectToLoad) { }

    public void LoadObject(object objectToLoad)
    {
        ViewSpecificLoad(objectToLoad: objectToLoad);
        LoadedObject = objectToLoad;
    }

    protected virtual void OnDirtyChanged(EventArgs e)
    {
        OnTitleNameChanged(e: new EventArgs());
        if (DirtyChanged != null)
        {
            DirtyChanged(sender: this, e: e);
        }
    }

    protected virtual void OnTitleNameChanged(EventArgs e)
    {
        this.Text = (IsDirty ? "*" : "") + _titleName;
        //this.TabText = this.TitleName;
        if (TitleNameChanged != null)
        {
            TitleNameChanged(sender: this, e: e);
        }
    }

    protected virtual void OnFileNameChanged(EventArgs e)
    {
        if (FileNameChanged != null)
        {
            FileNameChanged(sender: this, e: e);
        }
    }

    protected virtual void OnSaving(EventArgs e)
    {
        if (Saving != null)
        {
            Saving(sender: this, e: e);
        }
    }

    private void InitializeComponent()
    {
        //
        // AbstractViewContent
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(width: 5, height: 13);
        this.ClientSize = new System.Drawing.Size(width: 292, height: 273);
        this.DockAreas = DockAreas.Document;
        this.Name = "AbstractViewContent";
    }

    protected virtual void OnSaved(SaveEventArgs e)
    {
        if (Saved != null)
        {
            Saved(sender: this, e: e);
        }
    }

    public virtual object Content { get; set; }
    public event EventHandler TitleNameChanged;
    public event EventHandler FileNameChanged;
    public event EventHandler DirtyChanged;
    public event EventHandler Saving;
    public event SaveEventHandler Saved;
}
