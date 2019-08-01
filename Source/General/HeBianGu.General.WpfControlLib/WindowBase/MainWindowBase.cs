﻿using HeBianGu.Base.WpfBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace HeBianGu.General.WpfControlLib
{


    public partial class MainWindowBase : WindowBase
    {

        public ICommand NotifyWindowCommand { get; protected set; }

        public ICommand SettimgWindowCommand { get; protected set; }

        public MainWindowBase()
        {
            this.NotifyWindowCommand = new RoutedUICommand();

            this.BindCommand(NotifyWindowCommand, (l, k) =>
            {
                MessageService.ShowSnackMessageWithNotice("窗口即将隐藏至右下角，双击右下角图标显示窗口");

                this._notifyIcon.ShowBalloonTip(1000, "sssss", "sssss", NotifyBalloonIcon.Info);

                Task.Delay(1000).ContinueWith(t =>
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.ShowWindow = false;
                    });

                });
            });

            this.SettimgWindowCommand = new RoutedUICommand();
            this.BindCommand(SettimgWindowCommand, (s, e) =>
            {
                this.ShowWithLayer(e.Parameter as Uri);
            });

            this.ShowAnimation = l =>
            {
                // Todo ：初始化淡出初始效果 
                this.OpacityMask = this.FindResource("S.WindowOpMack.LoadBrush") as Brush;
            };

            this.CloseAnimation = l =>
            {
                this.BegionStoryClose();
            };

            this.BindCommand(CommandService.Close, async (s, e) =>
            {
                Action<object, DialogClosingEventArgs> action = (l, k) =>
                {
                    if ((bool)k.Parameter)
                    {
                        this.CloseAnimation?.Invoke(this);
                    }
                };

                if (Application.Current.MainWindow == this)
                {
                    await MessageService.ShowResultMessge("确认要退出系统?", action);
                }
                else
                {
                    this.CloseAnimation?.Invoke(this);
                }
            });
        }

        /// <summary> 托盘图标按钮图标 </summary>
        public ImageSource NotifyIconSource
        {
            get { return (ImageSource)GetValue(NotifyIconSourceProperty); }
            set { SetValue(NotifyIconSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotifyIconSourceProperty =
            DependencyProperty.Register("NotifyIconSource", typeof(ImageSource), typeof(MainWindowBase), new PropertyMetadata(default(ImageSource), (d, e) =>
            {
                MainWindowBase control = d as MainWindowBase;

                if (control == null) return;

                ImageSource config = e.NewValue as ImageSource;

            }));



        public bool ShowWindow
        {
            get { return (bool)GetValue(ShowWindowProperty); }
            set { SetValue(ShowWindowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowWindowProperty =
            DependencyProperty.Register("ShowWindow", typeof(bool), typeof(MainWindowBase), new PropertyMetadata(true, (d, e) =>
            {
                MainWindowBase control = d as MainWindowBase;

                if (control == null) return;

                bool config = (bool)e.NewValue;

                if (config)
                {
                    control.ShowOfScaleEnlarge();
                }
                else
                {
                    control.HideOfScaleReduce();
                }

            }));

        /// <summary> 用于重写关闭到那个花 </summary>
        public virtual void BegionStoryClose()
        {
            if (this._notifyIcon == null) return;

            this._notifyIcon.Visibility = Visibility.Collapsed;
            this._notifyIcon.Dispose();

            this.CloseDownToUpOps();

        }
    }

    [TemplatePart(Name = "PART_SnackBar", Type = typeof(Snackbar))]
    [TemplatePart(Name = "PART_SettingFrame", Type = typeof(ModernFrame))]
    [TemplatePart(Name = "PART_NotifyIcon", Type = typeof(NotifyIcon))]

    partial class MainWindowBase : IWindowBase
    {
        Snackbar _snackbar;
        ModernFrame _settingFrame;
        NotifyIcon _notifyIcon;
        public override void OnApplyTemplate()
        {

            base.OnApplyTemplate();

            this._snackbar = Template.FindName("PART_SnackBar", this) as Snackbar;
            this._settingFrame = Template.FindName("PART_SettingFrame", this) as ModernFrame;
            this._notifyIcon = Template.FindName("PART_NotifyIcon", this) as NotifyIcon;

            if (this._notifyIcon != null)
            {
                this._notifyIcon.MouseDoubleClick += (l, k) =>
                {
                    this.ShowWindow = !this.ShowWindow;

                };

            }

        }
        /// <summary> 输出消息 </summary>
        public void AddSnackMessage(string message)
        {
            SnackbarMessageQueue queue = _snackbar.MessageQueue;

            Task.Factory.StartNew(() => queue.Enqueue(message));
        }

        /// <summary> 输出消息和操作按钮 </summary>
        public void AddSnackMessage(string message, object actionContent, Action actionHandler)
        {
            SnackbarMessageQueue queue = _snackbar.MessageQueue;

            Task.Factory.StartNew(() => queue.Enqueue(message, actionContent, actionHandler));
        }

        /// <summary> 输出消息、按钮和参数 </summary>
        public void AddSnackMessage<TArgument>(string message, object actionContent, Action<TArgument> actionHandler,
            TArgument actionArgument)
        {
            SnackbarMessageQueue queue = _snackbar.MessageQueue;

            Task.Factory.StartNew(() => queue.Enqueue(message, actionContent, actionHandler, actionArgument));
        }


        public void ShowWithLayer(Uri uri, int layerIndex = 0)
        {
            _settingFrame.Source = uri as Uri;

            _settingFrame.Visibility = Visibility.Visible;
        }

        public void CloseWithLayer(int layerIndex = 0)
        {
            _settingFrame.Visibility = Visibility.Collapsed;
        }


        public void ShowNotifyMessage(string tipTitle, string tipText, NotifyBalloonIcon tipIcon = NotifyBalloonIcon.Info, int timeout = 1000)
        {
            this.Dispatcher.Invoke(() =>
            {
                _notifyIcon.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);

            });
        }
    }


    public interface IWindowBase
    {
        /// <summary> 输出消息 </summary>
        void AddSnackMessage(string message);

        /// <summary> 输出消息和操作按钮 </summary>
        void AddSnackMessage(string message, object actionContent, Action actionHandler);

        /// <summary> 输出消息、按钮和参数 </summary>
        void AddSnackMessage<TArgument>(string message, object actionContent, Action<TArgument> actionHandler,
           TArgument actionArgument);

        /// <summary> 显示蒙版 </summary>
        void ShowWithLayer(Uri uri, int layerIndex = 0);

        /// <summary> 关闭蒙版 </summary>
        void CloseWithLayer(int layerIndex = 0);


        /// <summary> 显示气泡消息 </summary>
        void ShowNotifyMessage(string tipTitle, string tipText, NotifyBalloonIcon tipIcon = NotifyBalloonIcon.Info, int timeout = 1000);

    }

}
