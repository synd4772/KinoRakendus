﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using KinoRakendus.core.utils;
using KinoRakendus.core.models;
using KinoRakendus.core.models.database;
using KinoRakendus.core.interfaces;
using KinoRakendus.core.enums;

namespace KinoRakendus.core.controls
{
    public partial class AdvancedOption<T>: UserControl where T: Table, ITable, new()
    {
        public Panel FieldPanel { get; set; }
        public Panel ValuePanel { get; set; }
        public Panel ButtonPanel { get; set; }
        public Label FieldLabel { get; set; } = new Label();
        public Label ValueLabel { get; set; } = new Label();
        public TextBox ValueTextBox { get; set; } = null;
        public SelectControl SelectControl { get; set; } = null;
        public Button Button { get; set; }
        public List<int> OptionSize { get; set;} = new List<int>() { 681, 48 };
        public List<int> FieldSize { get; set; } = new List<int>() { 191, 48};
        public List<int> ValueSize { get; set; } = new List<int>() { 442, 48};
        public List<int> ButtonSize { get; set; } = new List<int>() { 48, 48 };

        private bool _inChanging;
        public bool InChanging
        {
            get
            {
                return _inChanging;
            }
            set
            {
                if(_inChanging != value) _inChanging = value;
                StatusChanged();
            }
        }
        public int RecordId { get; set; }
        private string _fieldName;
        public string FieldName
        {
            get
            {
                return _fieldName;
            }
            set
            {
                _fieldName = value;
                FieldLabel.Text = _fieldName;
            }
        }
        private string _currentValue;
        public string CurrentValue
        {
            get
            {
                return _currentValue;
            }
            set
            {
                _currentValue = value;
                ValueLabel.Text = _currentValue;
            }
        }
        public T CurrentRecord { get; set; }
        public AdvancedOptionType Type { get; set; }
        public AdvancedOption(AdvancedOptionType type, int recordId, string fieldName, List<int> size = null , List<int> fieldSize = null, List<int> valueSize = null, List<int> buttonSize = null, List<SelectOption> options = null)
        {
            if(size != null) OptionSize = size;
            if(fieldSize != null) FieldSize = fieldSize;
            if(valueSize != null) ValueSize = valueSize;
            if(buttonSize != null) ButtonSize = buttonSize;
            Type = type;
            this.Size = new Size(OptionSize[0], OptionSize[1]);
            RecordId = recordId;
            FieldName = fieldName;
            CurrentRecord = DBHandler.GetRecord<T>(new List<WhereField>() { new WhereField("id", RecordId.ToString()) });
            CurrentValue = CurrentRecord[fieldName];
            InitAll();
            if(type == AdvancedOptionType.Select)
            {
                SelectControl = new SelectControl(ValueSize, options);
            }
            else if(type == AdvancedOptionType.TextBox)
            {
                InitTextBox();
            }
 
            InChanging = false;
        }
        private void clicked(object sender, EventArgs e)
        {
            
            if (InChanging)
            {
                if(Type == AdvancedOptionType.TextBox)
                {
                    ValueTextBox.Hide();
                    ValueLabel.Text = ValueTextBox.Text;
                    ValueLabel.Show();
                }
                else if(Type == AdvancedOptionType.Select)
                {
                    SelectControl.Hide();
                    SelectControl.HideDownBar();
                    ValueLabel.Show();
                }
            }
            else
            {
                if(Type == AdvancedOptionType.TextBox)
                {
                    Console.WriteLine($"Showing {ValueTextBox.Location.X} {ValueTextBox.Location.Y}");
                    ValueTextBox.Show();
                    ValueTextBox.Text = ValueLabel.Text;
                    ValueLabel.Hide();
                }
                else if(Type == AdvancedOptionType.Select)
                {
                    SelectControl.Show();
                    ValueLabel.Hide();
                }
            }
            InChanging = !InChanging;
            //DBHandler.UpdateRecord<T>(CurrentRecord, FieldName, )
        }
        public void StatusChanged()
        {
            if (InChanging)
            {
                Button.BackgroundImage = DefaultImages.GetDefaultImage("check.png");

            }
            else
            {
                Button.BackgroundImage = DefaultImages.GetDefaultImage("edit.png");
            }
        }
        public void InitAll()
        {
            InitField();
            InitValue();
            InitButton();
        }
        public void InitField()
        {
            FieldPanel = new Panel();
            FieldPanel.BackColor = ColorManagment.IconBackgroundPurple;
            FieldPanel.ClientSize = new Size(FieldSize[0], FieldSize[1]);
            this.Controls.Add(FieldPanel);

            FieldLabel = new Label();
            FieldLabel.BackColor = ColorManagment.InvisibleBackGround;
            FieldLabel.Font = DefaultFonts.GetFont(22);
            FieldLabel.ForeColor = ColorManagment.LightOptionsText;
            FieldLabel.Text = FieldName;
            FieldLabel.Size = new Size(FieldPanel.Width, FieldSize[1]);
            FieldLabel.TextAlign = ContentAlignment.MiddleLeft;
            FieldLabel.Location = new Point(0, 0);
            this.FieldPanel.Controls.Add(FieldLabel);
        }
        public void InitValue()
        {
            ValuePanel = new Panel();
            ValuePanel.BackColor = ColorManagment.OptionValueBackground;
            ValuePanel.Size = new Size(ValueSize[0], ValueSize[1]);
            ValuePanel.Location = new Point(FieldSize[0], 0);
            this.Controls.Add(ValuePanel);

            ValueLabel.Font = DefaultFonts.GetKanitFont(19);
            ValueLabel.BackColor = ColorManagment.InvisibleBackGround;
            ValueLabel.ForeColor = Color.White;
            ValueLabel.Size = new Size(ValuePanel.Width, ValuePanel.Height);
            ValuePanel.Controls.Add(ValueLabel);
            ValueLabel.TextAlign = ContentAlignment.MiddleLeft;
            ValueLabel.Location = new Point(0, 0);
        }
        public void InitTextBox()
        {
            ValueTextBox = new TextBox();
            ValueTextBox.AutoSize =false;
            ValueTextBox.BackColor = ColorManagment.InputColors;
            ValueTextBox.Font = DefaultFonts.GetFont(22);
            ValueTextBox.ForeColor = Color.White;
            ValueTextBox.ClientSize = new Size( ValuePanel.Width, ValuePanel.Height);
            ValueTextBox.Location = new Point(0, 0);
            ValueTextBox.Text = ValueLabel.Text;
            ValueTextBox.BorderStyle = BorderStyle.None;
            ValueTextBox.TextAlign = HorizontalAlignment.Center;
            ValueTextBox.Hide();
            ValuePanel.Controls.Add(ValueTextBox);
        }
        public void InitSelect()
        {
            SelectControl.Location = new Point(FieldSize[0], 0);
            SelectControl.Hide();
            this.Controls.Add(SelectControl);
        }
        public void InitButton()
        {
            ButtonPanel = new Panel();
            ButtonPanel.Size = new Size(ButtonSize[0], ButtonSize[1]);
            ButtonPanel.BackColor = ColorManagment.IconBackgroundPurple;
            ButtonPanel.Location = new Point(FieldSize[0] + ValueSize[0], 0);

            this.Controls.Add(ButtonPanel);

            Button = new Button();
            Button.Size = new Size(ButtonSize[0] - 20, ButtonSize[0] - 20);
            Button.BackgroundImageLayout = ImageLayout.Zoom;
            Button.FlatStyle = FlatStyle.Flat;
            Button.BackColor = ColorManagment.InvisibleBackGround;
            Button.FlatAppearance.BorderSize = 0;
            Button.Click += clicked;
            Button.FlatAppearance.MouseDownBackColor = ColorManagment.InvisibleBackGround;
            Button.FlatAppearance.MouseOverBackColor = ColorManagment.InvisibleBackGround;
            this.ButtonPanel.Controls.Add(Button);
            this.Button.Location = new Point(ButtonPanel.Width / 2 - Button.Width / 2, ButtonPanel.Height / 2 - Button.Height / 2);
        }
    }
}