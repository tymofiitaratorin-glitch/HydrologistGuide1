using HydrologistGuide1.Models;
using HydrologistGuide1.Services;
using HydrologistGuide1.Forms;
using HydrologistGuide1.Models;
using HydrologistGuide1.Services;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HydrologistGuide1.Forms;

/// <summary>
/// Main user interface of the hydrologist guide application.
/// </summary>
public class MainForm : Form
{
    private readonly HydrologyRepository _repository = new();
    private readonly BindingList<River> _view = new();
    private readonly DataGridView _grid = new();

    private readonly TextBox _nameSearchTextBox = new();
    private readonly TextBox _flowsIntoSearchTextBox = new();
    private readonly TextBox _waterObjectTextBox = new();

    private readonly Label _statusLabel = new();

    private readonly string _autoSaveFilePath;

    public MainForm()
    {
        _autoSaveFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "hydrology.json");

        Text = "Довідник гідролога";
        Width = 1180;
        Height = 720;
        StartPosition = FormStartPosition.CenterScreen;

        FormClosing += MainForm_FormClosing;

        BuildInterface();
        LoadDataOnStart();
        RefreshGrid(_repository.Rivers);
    }

    private void BuildInterface()
    {
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 1,
            Padding = new Padding(10)
        };

        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 75));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));

        var searchPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true
        };

        searchPanel.Controls.Add(new Label
        {
            Text = "Назва:",
            Width = 55,
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        });

        _nameSearchTextBox.Width = 170;
        searchPanel.Controls.Add(_nameSearchTextBox);

        searchPanel.Controls.Add(new Label
        {
            Text = "Куди впадає:",
            Width = 92,
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        });

        _flowsIntoSearchTextBox.Width = 170;
        searchPanel.Controls.Add(_flowsIntoSearchTextBox);

        var searchButton = new Button
        {
            Text = "Пошук",
            Width = 90
        };

        searchButton.Click += (_, _) => Search();

        var resetButton = new Button
        {
            Text = "Скинути",
            Width = 90
        };

        resetButton.Click += (_, _) =>
        {
            _nameSearchTextBox.Clear();
            _flowsIntoSearchTextBox.Clear();
            RefreshGrid(_repository.Rivers);
            _statusLabel.Text = "Пошук скинуто.";
        };

        searchPanel.Controls.Add(searchButton);
        searchPanel.Controls.Add(resetButton);

        searchPanel.Controls.Add(new Label
        {
            Text = "Море/озеро:",
            Width = 92,
            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        });

        _waterObjectTextBox.Width = 170;
        searchPanel.Controls.Add(_waterObjectTextBox);

        var waterCalcButton = new Button
        {
            Text = "Обчислити для водойми",
            Width = 170
        };

        waterCalcButton.Click += (_, _) => CalculateWaterObject();

        searchPanel.Controls.Add(waterCalcButton);

        mainPanel.Controls.Add(searchPanel, 0, 0);

        var commandPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill
        };
        AddButton(commandPanel, "Додати", AddRiver);
        AddButton(commandPanel, "Редагувати", EditRiver);
        AddButton(commandPanel, "Видалити", DeleteRiver);
        AddButton(commandPanel, "Обчислити для річки", CalculateSelectedRiver, 160);
        AddButton(commandPanel, "Зберегти", SaveData);
        AddButton(commandPanel, "Завантажити", LoadData, 120);

        mainPanel.Controls.Add(commandPanel, 0, 1);

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AutoGenerateColumns = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AllowUserToAddRows = false;

        AddGridColumn("Name", "Назва", 160);
        AddGridColumn("LengthKm", "Довжина, км", 110);
        AddGridColumn("FlowsIntoName", "Куди впадає", 160);
        AddGridColumn("MouthType", "Тип", 90);
        AddGridColumn("AnnualRunoffKm3", "Стік, км³/рік", 120);
        AddGridColumn("BasinAreaKm2", "Басейн, км²", 130);

        _grid.DataSource = _view;

        mainPanel.Controls.Add(_grid, 0, 2);

        _statusLabel.Dock = DockStyle.Fill;
        _statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        mainPanel.Controls.Add(_statusLabel, 0, 3);

        Controls.Add(mainPanel);
    }

    private static void AddButton(FlowLayoutPanel panel, string text, Action action, int width = 110)
    {
        var button = new Button
        {
            Text = text,
            Width = width,
            Height = 32
        };

        button.Click += (_, _) => action();

        panel.Controls.Add(button);
    }

    private void AddGridColumn(string property, string header, int width)
    {
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = property,
            HeaderText = header,
            Width = width
        });
    }

    private River? SelectedRiver()
    {
        return _grid.CurrentRow?.DataBoundItem as River;
    }

    private void AddRiver()
    {
        using var form = new RiverEditForm(new River(), _repository.Rivers.ToArray());

        if (form.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            _repository.Add(form.River);
            AutoSaveData();
            RefreshGrid(_repository.Rivers);
            _statusLabel.Text = "Річку додано. Дані автоматично збережено.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Помилка додавання",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void EditRiver()
    {
        var selected = SelectedRiver();

        if (selected == null)
        {
            MessageBox.Show("Оберіть річку для редагування.");
            return;
        }

        using var form = new RiverEditForm(selected, _repository.Rivers.ToArray());

        if (form.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            _repository.Update(form.River);
            AutoSaveData();
            RefreshGrid(_repository.Rivers);
            _statusLabel.Text = "Річку оновлено. Дані автоматично збережено.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Помилка редагування",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void DeleteRiver()
    {
        var selected = SelectedRiver();

        if (selected == null)
        {
            MessageBox.Show("Оберіть річку для видалення.");
            return;
        }

        var answer = MessageBox.Show(
            $"Видалити річку {selected.Name}?",
            "Підтвердження",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (answer != DialogResult.Yes)
            return;
        try
        {
            _repository.Delete(selected.Id);
            AutoSaveData();
            RefreshGrid(_repository.Rivers);
            _statusLabel.Text = "Річку видалено. Дані автоматично збережено.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Помилка видалення",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void Search()
    {
        var result = _repository.Search(
            _nameSearchTextBox.Text,
            _flowsIntoSearchTextBox.Text,
            null,
            null,
            null);

        RefreshGrid(result);

        _statusLabel.Text = _view.Count == 0
            ? "Річки за умовами пошуку не знайдено."
            : $"Знайдено записів: {_view.Count}.";
    }

    private void CalculateSelectedRiver()
    {
        var selected = SelectedRiver();

        if (selected == null)
        {
            MessageBox.Show("Оберіть річку для обчислення.");
            return;
        }

        var calculator = new HydrologyCalculator(_repository.Rivers);
        var result = calculator.CalculateForRiver(selected.Id);

        ShowCalculation(result);
    }

    private void CalculateWaterObject()
    {
        try
        {
            var calculator = new HydrologyCalculator(_repository.Rivers);
            var result = calculator.CalculateForWaterObject(_waterObjectTextBox.Text);

            ShowCalculation(result);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Помилка обчислення",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void ShowCalculation(BasinCalculationResult result)
    {
        string riverList = result.IncludedRivers.Count == 0
            ? "Не знайдено річок для обчислення."
            : string.Join(Environment.NewLine, result.IncludedRivers.Select(r => "- " + r));

        MessageBox.Show(
            result + Environment.NewLine + Environment.NewLine + riverList,
            "Результат обчислення",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void SaveData()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "JSON файли (*.json)|*.json|Усі файли (*.*)|*.*",
            FileName = "hydrology.json"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            _repository.Save(dialog.FileName);
            _repository.Save(_autoSaveFilePath);
            _statusLabel.Text = "Дані збережено у файл.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Помилка збереження",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void LoadData()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "JSON файли (*.json)|*.json|Усі файли (*.*)|*.*"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            _repository.Load(dialog.FileName);
            AutoSaveData();
            RefreshGrid(_repository.Rivers);
            _statusLabel.Text = "Дані завантажено з файлу і автоматично збережено для наступного запуску.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Помилка завантаження",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }

    private void RefreshGrid(System.Collections.Generic.IEnumerable<River> rivers)
    {
        _view.Clear();

        foreach (var river in rivers.OrderBy(r => r.Name))
            _view.Add(river.Clone());
    }
    private void LoadDataOnStart()
    {
        try
        {
            if (File.Exists(_autoSaveFilePath))
            {
                _repository.Load(_autoSaveFilePath);
                _statusLabel.Text = "Дані автоматично завантажено з файлу.";
            }
            else
            {
                var seed = SeedData.Create();
                _repository.ReplaceAll(seed);
                AutoSaveData();
                _statusLabel.Text = "Створено початковий файл даних.";
            }
        }
        catch
        {
            var seed = SeedData.Create();
            _repository.ReplaceAll(seed);
            AutoSaveData();
            _statusLabel.Text = "Файл даних було пошкоджено. Завантажено початкові дані.";
        }
    }

    private void AutoSaveData()
    {
        _repository.Save(_autoSaveFilePath);
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        try
        {
            AutoSaveData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Помилка автоматичного збереження",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}