using HydrologistGuide1.Models;
using HydrologistGuide1.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace HydrologistGuide1.Forms;

/// <summary>
/// Modal form for adding and editing a river record.
/// </summary>
public class RiverEditForm : Form
{
    private readonly TextBox _nameTextBox = new();
    private readonly TextBox _lengthTextBox = new();
    private readonly TextBox _flowsIntoTextBox = new();
    private readonly ComboBox _mouthTypeComboBox = new();
    private readonly TextBox _runoffTextBox = new();
    private readonly TextBox _basinTextBox = new();
    private readonly ComboBox _parentComboBox = new();
    private readonly Label _errorLabel = new();
    private readonly River[] _availableRivers;

    public River River { get; private set; }

    public RiverEditForm(River river, River[] availableRivers)
    {
        River = river.Clone();
        _availableRivers = availableRivers;

        Text = string.IsNullOrWhiteSpace(river.Name) ? "Додавання річки" : "Редагування річки";
        Width = 520;
        Height = 430;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        BuildInterface();
        FillFields();
    }

    private void BuildInterface()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 9,
            Padding = new Padding(12),
            AutoSize = true
        };

        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(table, 0, "Назва річки", _nameTextBox);
        AddRow(table, 1, "Довжина, км", _lengthTextBox);
        AddRow(table, 2, "Куди впадає", _flowsIntoTextBox);
        AddRow(table, 3, "Тип гирла", _mouthTypeComboBox);
        AddRow(table, 4, "Річний стік, км³/рік", _runoffTextBox);
        AddRow(table, 5, "Площа басейну, км²", _basinTextBox);
        AddRow(table, 6, "Є притокою річки", _parentComboBox);

        _mouthTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _mouthTypeComboBox.DataSource = Enum.GetValues(typeof(MouthType));

        _parentComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _parentComboBox.DisplayMember = "Name";
        _parentComboBox.ValueMember = "Id";

        var parentItems = _availableRivers
            .Where(r => r.Id != River.Id)
            .OrderBy(r => r.Name)
            .ToList();

        parentItems.Insert(0, new River
        {
            Id = Guid.Empty,
            Name = "Не є притокою"
        });

        _parentComboBox.DataSource = parentItems;

        _errorLabel.ForeColor = System.Drawing.Color.DarkRed;
        _errorLabel.AutoSize = true;

        table.Controls.Add(_errorLabel, 0, 7);
        table.SetColumnSpan(_errorLabel, 2);

        var buttonsPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill
        };

        var okButton = new Button
        {
            Text = "OK",
            Width = 90
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            Width = 90,
            DialogResult = DialogResult.Cancel
        };

        okButton.Click += OkButton_Click;

        buttonsPanel.Controls.Add(okButton);
        buttonsPanel.Controls.Add(cancelButton);

        table.Controls.Add(buttonsPanel, 0, 8);
        table.SetColumnSpan(buttonsPanel, 2);

        Controls.Add(table);

        AcceptButton = okButton;
        CancelButton = cancelButton;
    }

    private static void AddRow(TableLayoutPanel table, int row, string caption, Control control)
    {
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        var label = new Label
        {
            Text = caption,
            Dock = DockStyle.Fill,
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        };

        control.Dock = DockStyle.Fill;

        table.Controls.Add(label, 0, row);
        table.Controls.Add(control, 1, row);
    }

    private void FillFields()
    {
        _nameTextBox.Text = River.Name;
        _lengthTextBox.Text = River.LengthKm.ToString(CultureInfo.InvariantCulture);
        _flowsIntoTextBox.Text = River.FlowsIntoName;
        _mouthTypeComboBox.SelectedItem = River.MouthType;
        _runoffTextBox.Text = River.AnnualRunoffKm3.ToString(CultureInfo.InvariantCulture);
        _basinTextBox.Text = River.BasinAreaKm2.ToString(CultureInfo.InvariantCulture);
        _parentComboBox.SelectedValue = River.ParentRiverId ?? Guid.Empty;
    }

    private void OkButton_Click(object? sender, EventArgs e)
    {
        _errorLabel.Text = string.Empty;

        if (!double.TryParse(
                _lengthTextBox.Text.Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double length))
        {
            _errorLabel.Text = "Довжина має бути числом.";
            return;
        }

        if (!double.TryParse(
                _runoffTextBox.Text.Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double runoff))
        {
            _errorLabel.Text = "Річний стік має бути числом.";
            return;
        }

        if (!double.TryParse(
                _basinTextBox.Text.Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double basin))
        {
            _errorLabel.Text = "Площа басейну має бути числом.";
            return;
        }

        River.Name = _nameTextBox.Text.Trim();
        River.LengthKm = length;
        River.FlowsIntoName = _flowsIntoTextBox.Text.Trim();
        River.MouthType = (MouthType)_mouthTypeComboBox.SelectedItem!;
        River.AnnualRunoffKm3 = runoff;
        River.BasinAreaKm2 = basin;

        var parentId = (Guid)_parentComboBox.SelectedValue!;
        River.ParentRiverId = parentId == Guid.Empty ? null : parentId;

        DialogResult = DialogResult.OK;
        Close();
    }
}
