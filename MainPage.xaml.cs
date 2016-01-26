using _51Wp.XinFengSDK.UWP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace XF_Sdk_UWP_Demo
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            //初始化统计SDK
            //见App.xaml.cs中OnLaunched()方法最后一行代码
            //初始化登录SDK
            AccountManager.AccountInit("328", "500375", grid_Root, true, 0);
            //初始化支付SDK
            _51Wp.XinFengSDK.UWP.Pay.PaymentManager.Initialize("328", "500375", grid_Root);

            //登录结果回调事件
            AccountSdkCallBack();

        }


        #region 登录相关的操作
        private void AccountSdkCallBack()
        {

            AccountManager.LoginStateCallback += async (o, result) =>
            {
                if (result != null)
                {
                    if (result.ReturnCode == "0")
                    {
                        //此处增加防伪造登录校验
                        string strUrl = "http://www.51wp.com/winphone/index.php?s=/Win10oauth2/checkAccount&uid={0}&datetime={1}&uniqueid={2}";
                        var token = result.Token;
                        var times = result.LoginTime;
                        var uid = result.UserId;
                        string urlPath = string.Format(strUrl, new string[] { uid, times, token });

                        var webClient = new HttpClient();


                        // webClient.DownloadStringAsync(new Uri(uri, UriKind.Absolute));
                        HttpResponseMessage response = await webClient.GetAsync(urlPath);
                        string resultstr = await response.Content.ReadAsStringAsync();
                        try
                        {
                            if (resultstr != null)
                            {
                                if (resultstr == "0")//0为正常登录，非0为伪造登录
                                {

                                    var dialog = new MessageDialog("登录成功，玩家信息：username=" + result.UserName + ",userId=" + result.UserId);
                                    await dialog.ShowAsync();
                                }
                                else
                                {
                                    var dialog = new MessageDialog("登录失败，为伪造登录！");
                                    await dialog.ShowAsync();
                                }

                            }

                        }
                        catch
                        {

                        }


                    }
                    else
                    {
                        var dialog = new MessageDialog("登录失败，请稍后重试！");
                        await dialog.ShowAsync();
                    }
                }
                else
                {
                    var dialog = new MessageDialog("登录遇到了问题，请稍后重试！");
                    await dialog.ShowAsync();
                }
            };
            AccountManager.LoginChangeAccountCallback += async (sender, args) =>
            {
                var dialog = new MessageDialog("用户点击了切换帐号按钮！");
                await dialog.ShowAsync();
            };
            AccountManager.LoginCloseCallback += async (sender, args) =>
            {
                var dialog = new MessageDialog("用户手动关闭了登录页面！");
                await dialog.ShowAsync();
            };
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _51Wp.XinFengSDK.UWP.AccountManager.BeginLogin();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _51Wp.XinFengSDK.UWP.AccountManager.ChangeLogin();
            //_51Wp.XinFengSDK.UWP.Pay.PaymentManager.Initialize("821", "500346", grid_Root);
            //_51Wp.XinFengSDK.UWP.Pay.PaymentManager.CreateOrderAndPay(_51Wp.XinFengSDK.UWP.Pay.Common.PlayerType.Anonymous, "playerid.Text", "821500346002", "AlternateField.Text", "GameServer.Text");

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {


            Button bt = sender as Button;
            if (bt.Content.ToString() == "隐藏浮层")
            {
                bt.Content = "显示浮层";
                _51Wp.XinFengSDK.UWP.AccountManager.HideSmallAccoutLogo(true);
            }
            else
            {
                bt.Content = "隐藏浮层";
                _51Wp.XinFengSDK.UWP.AccountManager.HideSmallAccoutLogo(false);

            }
        }
        #endregion


        #region 支付相关的操作
        _51Wp.XinFengSDK.UWP.Pay.Common.PlayerType PlayerType;
        private async void PaymentManager_PaymentResultCallback(object sender, _51Wp.XinFengSDK.UWP.Pay.Features.AsyncProcessResult e)
        {
            if (e != null)
            {
                if (e.ErrorCode == 0)
                {
                    var msgbox = new MessageDialog("订单号: " + e.OrderId + "\r\n商品编号: " + e.GoodsId + "\r\n商品名称: " + e.GoodsName + "\r\n商品价格: " + e.GoodsPrice + "\r\n玩家帐号: " + e.PlayerId + "\r\n游戏唯一编号: " + e.GameId + " \r\n订单状态：支付成功，可以发放道具商品了", "支付成功");
                    await msgbox.ShowAsync();
                }
                else
                {
                    var msgbox = new MessageDialog("订单号: " + e.OrderId + "\r\n商品编号: " + e.GoodsId + "\r\n商品名称: " + e.GoodsName + "\r\n商品价格: " + e.GoodsPrice + "\r\n玩家帐号: " + e.PlayerId + "\r\n游戏唯一编号: " + e.GameId + " \r\n订单状态：支付失败，支付未能完成", "支付失败");
                    await msgbox.ShowAsync();
                }
            }
        }

        private async void Button_Click_22(object sender, RoutedEventArgs e)
        {
            var result = await _51Wp.XinFengSDK.UWP.Pay.PaymentManager.GetGameGoodsListAsync();
            if (result != null)
            {
                if (result.code == 200)
                {
                    foreach (var item in result.data)
                    {
                        GoodsList.Items.Add("商品属性:" + item.propsId + " |" + item.propsName + "|" + item.propsPrice.ToString());

                    }
                }
                else
                {
                    var dialog = new MessageDialog("没有获取到该游戏下的商品信息,请核对接入参数");
                    await dialog.ShowAsync();
                }
            }
            else
            {
                var dialog = new MessageDialog("获取商品信息失败，请检查你的网络状态");
                await dialog.ShowAsync();
            }
            //默认选择第一行道具
            if (GoodsList.Items.Count > 0)
            {
                GoodsList.SelectedIndex = 0;
            }
        }



        private void GoodsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string goodsprop = GoodsList.SelectedItem as string;
            goodsId.Text = goodsprop.Split(':')[1].Split('|')[0];//选择相应的道具ID进行支付
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            PlayerType = _51Wp.XinFengSDK.UWP.Pay.Common.PlayerType.Anonymous;
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            PlayerType = _51Wp.XinFengSDK.UWP.Pay.Common.PlayerType.RegistedUser;
        }

        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            _51Wp.XinFengSDK.UWP.Pay.PaymentManager.CreateOrderAndPay(PlayerType, playerid.Text, goodsId.Text, AlternateField.Text, GameServer.Text);
        }
        #endregion

    }
}
