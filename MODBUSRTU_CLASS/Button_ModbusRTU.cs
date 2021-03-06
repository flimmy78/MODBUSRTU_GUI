﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MODBUSRTU_CLASS
{
    public partial class Button_ModbusRTU : Button
    {
        public Button_ModbusRTU()
        {
            InitializeComponent();
            PeriodicRefreshTimer.Elapsed += PeriodicRefreshTimer_Elapsed;
            PeriodicRefreshTimer.Enabled = true;
            this.MouseClick += Button_MouseClick;
            ModbusRTU.VoteToConfirmTransmitRegs('+', (byte)_ReadFunctionCode, _ReadAddress, _ReadDataLengthInBit);//控件初始化时，票决器根据地址值自动加1，并判断票选结果
        }

        private void Button_MouseClick(object sender, MouseEventArgs e)
        {
            this.PeriodicRefreshTimer.Enabled = false;
            ModbusRTU.SuspendRead((byte)_ReadFunctionCode, _ReadAddress, _ReadDataLengthInBit);//鼠标单击本控件后，进入写寄存器状态，暂停寄存器的读操作

            switch (_ButtonClickAction)
            {
                case ModbusButtonClickActionEnum.Set:
                    ModbusRTU.MasterDataRepos.Coils[_WriteAddress] = true;
                    break;

                case ModbusButtonClickActionEnum.Reset:
                    ModbusRTU.MasterDataRepos.Coils[_WriteAddress] = false;
                    break;

                case ModbusButtonClickActionEnum.Toggle:
                    ModbusRTU.MasterDataRepos.Coils[_WriteAddress] = !ModbusRTU.MasterDataRepos.Coils[_WriteAddress];
                    break;

                default:
                    break;

            }
            ModbusRTU.VoteToConfirmTransmitRegs('+', (byte)_WriteFunctionCode, _WriteAddress, _WriteDataLengthInBit);
            ModbusRTU.AssembleRequestADU(1, ModbusRTU.LoadUnmannedBuses((byte)_WriteFunctionCode, 0));
            ModbusRTU.ResumeRead((byte)_ReadFunctionCode, _ReadAddress, _ReadDataLengthInBit);//鼠标单击本控件后，进入写寄存器状态，暂停寄存器的读操作
            PeriodicRefreshTimer.Enabled = true;
        }

        private System.Timers.Timer PeriodicRefreshTimer = new System.Timers.Timer();

        #region"/////////////////////自定义的属性///////////////////"
        private byte _StationID = 1;
        private UInt16 _WriteAddress = 0;
        private UInt16 _ReadAddress = 0;
        private string _IfTrueText = "On";
        private string _IfFalseText = "Off";
        private ModbusButtonClickActionEnum _ButtonClickAction = ModbusButtonClickActionEnum.Set;
        private byte _ReadDataLengthInBit = 1;//读取长度为1
        private byte _WriteDataLengthInBit = 1;
        private WriteFunctionCodeEnum _WriteFunctionCode = WriteFunctionCodeEnum.WriteSingleCoil;
        private ReadFunctionCodeEnum _ReadFunctionCode = ReadFunctionCodeEnum.ReadCoils;


        [Category("ModbusRTU"), Description("读取的位值为真时，button上显示的字符串")]
        public string IfTrueText
        {
            get
            {
                return _IfTrueText;
            }
            set
            {
                _IfTrueText = value;
            }
        }

        [Category("ModbusRTU"), Description("读取的位值为真时，button上显示的字符串")]
        public string IfFalseText
        {
            get
            {
                return _IfFalseText;
            }
            set
            {
                _IfFalseText = value;
            }
        }


        [Category("ModbusRTU"), Description("读取数据的长度（以位为单位）")]
        public byte ReadDataLengthInBit
        {
            get
            {
                return _ReadDataLengthInBit;//总是为1 
            }
        }

        [Category("ModbusRTU"), Description("输出数据的长度（以位为单位）")]
        public byte WriteDataLengthInBit
        {
            get
            {
                return _WriteDataLengthInBit;//总是为1
            }
        }



        [Category("ModbusRTU"), Description("查询周期")]
        public double RefreshPeriod
        {
            get
            {
                return PeriodicRefreshTimer.Interval;
            }
            set
            {
                PeriodicRefreshTimer.Interval = value;
            }
        }
        [Category("ModbusRTU"), Description("周期查询使能")]
        public bool EnablePeriodRefresh
        {
            get
            {
                return PeriodicRefreshTimer.Enabled;
            }
            set
            {
                PeriodicRefreshTimer.Enabled = value;
            }
        }


        [Category("ModbusRTU"), Description("从站号（ID）")]
        public byte StationID
        {
            set
            {
                _StationID = value;
            }

            get
            {
                return _StationID;
            }

        }

        [Category("ModbusRTU"), Description("写入地址（WriteAddress）")]
        public UInt16 WriteAddress
        {
            set
            {
                if (value > 65535) value = 65535;  //对地址范围加以框定，否则在运行中输入超出范围的数据会弹出异常并中断执行
                if (value < 0) value = 0;             //地址范围本来就是0-65535，进行此框定也是合理的
                _WriteAddress = value;

            }
            get
            {
                return _WriteAddress;
            }

        }

        [Category("ModbusRTU"), Description("读取地址（ReadAddress）")]
        public UInt16 ReadAddress
        {
            set
            {
                ModbusRTU.VoteToConfirmTransmitRegs('-', (byte)_ReadFunctionCode, _ReadAddress, _ReadDataLengthInBit);//上次地址所对应的表决器值减1并判断票选结果

                if (value > 65535) value = 65535;  //对地址范围加以框定，否则在运行中输入超出范围的数据会弹出异常并中断执行
                if (value < 0) value = 0;             //地址范围本来就是0-65535，进行此框定也是合理的
                _ReadAddress = value;//新设置的地址存入地址属性

                ModbusRTU.VoteToConfirmTransmitRegs('+', (byte)_ReadFunctionCode, _ReadAddress, _ReadDataLengthInBit);//本次地址所对应的表决器值加1并判断票选结果

            }
            get
            {
                return _ReadAddress;
            }

        }

        [Category("ModbusRTU"), Description("读取指令（功能码）")]
        public ReadFunctionCodeEnum ReadFuncCode
        {
            get
            {
                return _ReadFunctionCode;
            }

        }

        [Category("ModbusRTU"), Description("写指令（功能码）")]
        public WriteFunctionCodeEnum WriteFuncCode
        {
            get
            {
                return _WriteFunctionCode;
            }

        }


        [Category("ModbusRTU"), Description("Modbus按键动作")]
        public ModbusButtonClickActionEnum ButtonClickAction
        {
            get
            {
                return _ButtonClickAction;
            }

            set
            {
                _ButtonClickAction = value;
            }
        }

        #endregion


        private void PeriodicRefreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            string StringInText;

            if (ModbusRTU.MasterDataRepos.RCoilFlag[_ReadAddress] == false)
                return;
            else
            {
                if (ModbusRTU.MasterDataRepos.Coils[_ReadAddress] == true)
                    StringInText = _IfTrueText;
                else
                    StringInText = _IfFalseText;
            }

            try//防止调用此控件的窗体没有显示出来之前可能发生的周期刷新事件导致的异常发生
            {
                this.Invoke((EventHandler)(delegate  //解决线程间调用显示的问题   可能存在线程间等待的问题，需要确认并优化
                {
                    this.Text = StringInText;
                }));
            }
            catch (Exception)
            {
                //无需操作，直接忽略
            }


        }


        #region"////////////////////////枚举量或结构体////////////////////////"
        public enum ReadFunctionCodeEnum
        {
            [Description("读线圈")]
            ReadCoils = 0x01,

        }

        public enum WriteFunctionCodeEnum
        {
            [Description("写单个线圈")]
            WriteSingleCoil = 0x05,
        }

        public enum ModbusButtonClickActionEnum
        {
            [Description("置1")]
            Set = 0x01,
            [Description("置0")]
            Reset = 0x00,
            [Description("翻转位值")]
            Toggle = 0x03
        }
        #endregion

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }
    }
}
