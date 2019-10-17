﻿using HeBianGu.Base.WpfBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HeBianGu.General.WpfControlLib
{
    public static class MessageService
    {

        public static MessageCloseLayerCommand CloseLayer { get; } = new MessageCloseLayerCommand();

        #region - 提示消息 -

        public static void ShowSnackMessage(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IWindowBase window = Application.Current.MainWindow as IWindowBase;

                if (window != null)
                {
                    window.AddSnackMessage(message);
                }
            });
        }

        public static void ShowSnackMessageWithNotice(string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IWindowBase window = Application.Current.MainWindow as IWindowBase;

                if (window != null)
                {
                    window.AddSnackMessage($"友情提示：[" + DateTime.Now.ToString("HH:mm:ss]  " + message));
                }
            });
        }

        /// <summary> 输出消息和操作按钮 </summary>
        public static void ShowSnackMessage(string message, object actionContent, Action actionHandler)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IWindowBase window = Application.Current.MainWindow as IWindowBase;

                if (window != null)
                {
                    window.AddSnackMessage(message, actionContent, actionHandler);
                }
            });

        }

        /// <summary> 输出消息、按钮和参数 </summary>
        public static void ShowSnackMessage<TArgument>(string message, object actionContent, Action<TArgument> actionHandler,
            TArgument actionArgument)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IWindowBase window = Application.Current.MainWindow as IWindowBase;

                if (window != null)
                {
                    window.AddSnackMessage(message, actionContent, actionHandler, actionArgument);
                }
            });

        }


        #endregion

        #region - 窗口消息 -

        public static async Task ShowWaittingMessge(Action action, Action closeAction = null)
        {
            await Application.Current.Dispatcher.Invoke(async () =>
             {
                 if (CheckOpen()) return null;

                 var view = new WaittingMessageDialog();

                 //show the dialog
                 return await DialogHost.ShowWithOpen(view, "windowDialog", (l, e) =>
                 {
                     Task.Run(action).ContinueWith(m =>
                     {
                         Application.Current.Dispatcher.Invoke(() =>
                         {
                             e.Session.Close(false);

                             closeAction?.Invoke();
                         });
                     });
                 });
             });
        }

        //public static async Task<object> ShowWaittingMessge(Func<object> action, Action closeAction = null)
        //{
        //    await Application.Current.Dispatcher.Invoke(async () =>
        //    {
        //        if (CheckOpen()) return null;

        //        var view = new WaittingMessageDialog();

        //        //show the dialog
        //        return await DialogHost.ShowWithOpen(view, "windowDialog", (l, e) =>
        //        {
        //          Task.Run(action);
        //            //.ContinueWith(m =>
        //            //{
        //            //    Application.Current.Dispatcher.Invoke(() =>
        //            //    {
        //            //        e.Session.Close(false);

        //            //        closeAction?.Invoke();
        //            //    });
        //            //});
        //        });
        //    });
        //}

        public static async Task ShowPercentProgress(Action<IPercentProgress> action, Action closeAction = null)
        {
            await ShowProgressMessge<PercentProgressDialog>(action, closeAction);
        }

        public static async Task ShowStringProgress(Action<StringProgressDialog> action, Action closeAction = null)
        {
            await ShowProgressMessge(action, closeAction);
        }

        public static async Task ShowProgressMessge<T>(Action<T> action, Action closeAction = null) where T : new()
        {
            if (CheckOpen()) return;


            await Application.Current.Dispatcher.Invoke(async () =>
            {
                var view = new T();

                //show the dialog
                return await DialogHost.ShowWithOpen(view, "windowDialog", (l, e) =>
                {
                    Task.Run(() => action.Invoke(view)).ContinueWith(m =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            e.Session.Close(false);

                            closeAction?.Invoke();
                        });
                    });
                });
            });
        }

        public static async Task ShowSumitMessge(string message)
        {
            if (CheckOpen()) return;

            await Application.Current.Dispatcher.Invoke(async () =>
             {
                 var view = new SampleMessageDialog();

                 view.MessageStr = message;

                 return await DialogHost.Show(view, "windowDialog");
             });
        }

        /// <summary> 显示自定义窗口 </summary>
        public static async Task<object> ShowCustomDialog(UIElement element, Action<object, DialogClosingEventArgs> action = null)
        {
            if (CheckOpen()) return null;

            return await Application.Current.Dispatcher.Invoke(async () =>
             {
                 //show the dialog
                 return await DialogHost.ShowWithClose(element, "windowDialog", (l, e) =>
                  {
                      action?.Invoke(l, e);
                  });
             });
        }

        static bool CheckOpen()
        {
            if (IsOpened())
            {
                ShowSnackMessageWithNotice("存在未关闭的窗口，请等待或关闭窗口再执行此操作");
                return true;
            }

            return false;
        }

        public static bool IsOpened()
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                return DialogHost.IsOpened();
            });

        }

        public static async Task ShowResultMessge(string message, Action<object, DialogClosingEventArgs> action)
        {
            if (CheckOpen()) return;

            await Application.Current.Dispatcher.Invoke(async () =>
             {
                 var view = new ResultMessageDialog();

                 view.MessageStr = message;

                 //show the dialog
                 return await DialogHost.ShowWithClose(view, "windowDialog", (l, e) =>
                  {
                      action?.Invoke(l, e);
                  });
             });

        }

        public static async Task<bool> ShowResultMessge(string message)
        {
            if (CheckOpen()) return false;

            bool result = false;

            Action<object, DialogClosingEventArgs> action = (l, k) =>
             {
                 result = (bool)k.Parameter;
             };

            await Application.Current.Dispatcher.Invoke(async () =>
            {
                var view = new ResultMessageDialog();

                view.MessageStr = message;

                //show the dialog
                return await DialogHost.ShowWithClose(view, "windowDialog", (l, e) =>
                {
                    action?.Invoke(l, e);
                });
            });

            return result;

        }

        #endregion

        #region - 蒙版消息 -

        /// <summary> 显示蒙版 </summary>
        public static void ShowWithLayer(Uri uri, int layerIndex = 0)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is IWindowBase window)
                {
                    window.ShowWithLayer(uri);
                }
            });
        }

        /// <summary> 显示蒙版 </summary>
        public static void ShowWithLayer(IActionResult link, int layerIndex = 0)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is IWindowBase window)
                {
                    window.ShowWithLayer(link);
                }
            });
        }


        /// <summary> 显示蒙版 </summary>
        public static void ShowWithLayer(FrameworkElement element, int layerIndex = 0)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is IWindowBase window)
                {
                    window.ShowWithLayer(element);
                }
            });
        }

        /// <summary> 关闭蒙版 </summary>
        public static void CloseWithLayer(int layerIndex = 0)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is IWindowBase window)
                {
                    window.CloseWithLayer();
                }
            });
        }

        #endregion

        #region - 气泡消息 -

        /// <summary> 显示气泡消息 </summary>
        public static void ShowNotifyMessage(string message, string title = null, NotifyBalloonIcon tipIcon = NotifyBalloonIcon.Info, int timeout = 1000)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IWindowBase window = Application.Current.MainWindow as IWindowBase;

                if (window != null)
                {
                    window.ShowNotifyMessage(title, message, tipIcon, timeout);
                }
            });

        }

        /// <summary> 显示自定义气泡消息 </summary>
        public static void ShowNotifyDialogMessage(string message, string title = null, int closeTime = -1)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                NotifyDialogWindow.ShowMessage(message, title, closeTime);
            });
        }

        #endregion


        #region - 列表消息 -

        static NotifyMessageWindow _notifyMessage;

        /// <summary> 显示自定义气泡消息 </summary>
        public static void ShowSystemNotifyMessage(MessageBase message)
        {
            if (_notifyMessage == null)
            {
                _notifyMessage = new NotifyMessageWindow();

                _notifyMessage.Show();
            }

            _notifyMessage.Source.Add(message);
        }

        /// <summary> 输出消息、按钮和参数 </summary>
        public static void ShowWindowNotifyMessage(MessageBase message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IWindowBase window = Application.Current.MainWindow as IWindowBase;

                if (window != null)
                {
                    window.ShowWindowNotifyMessage(message);
                }
            });
        }

        public static void ShowSystemNotifyMessageWithSuccess(string message)
        {
            ShowSystemNotifyMessage(new SuccessMessage
            {
                Message = message
            });
        }

        public static void ShowSystemNotifyMessageWithInfo(string message)
        {
            ShowSystemNotifyMessage(new InfoMessage
            {
                Message = message
            });
        }

        public static void ShowSystemNotifyMessageWithError(string message)
        {
            ShowSystemNotifyMessage(new ErrorMessage
            {
                Message = message
            });
        }

        public static void ShowSystemNotifyMessageWithWarn(string message)
        {
            ShowSystemNotifyMessage(new WarnMessage
            {
                Message = message
            });
        }

        public static void ShowSystemNotifyMessageWithFatal(string message)
        {
            ShowSystemNotifyMessage(new FatalMessage
            {
                Message = message
            });
        }

        public static void ShowWindowNotifyMessageWithSuccess(string message)
        {
            ShowWindowNotifyMessage(new SuccessMessage
            {
                Message = message
            });
        }

        public static void ShowWindowNotifyMessageWithInfo(string message)
        {
            ShowWindowNotifyMessage(new InfoMessage
            {
                Message = message
            });
        }

        public static void ShowWindowNotifyMessageWithError(string message)
        {
            ShowWindowNotifyMessage(new ErrorMessage
            {
                Message = message
            });
        }

        public static void ShowWindowNotifyMessageWithWarn(string message)
        {
            ShowWindowNotifyMessage(new WarnMessage
            {
                Message = message
            });
        }

        public static void ShowWindowNotifyMessageWithFatal(string message)
        {
            ShowWindowNotifyMessage(new FatalMessage
            {
                Message = message
            });
        }

        #endregion

    }

    public class MessageCloseLayerCommand : ICommand
    {

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            MessageService.CloseWithLayer();
        }

        public event EventHandler CanExecuteChanged;
    }
}
