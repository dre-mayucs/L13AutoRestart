using System;
using System.IO;
using System.Net.NetworkInformation;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace L13AutoRestart
{
    class Program
    {
        private struct AutoRestartConfig
        {
            /// <summary> ホーム画面のURL </summary>
            public string HomeUrl { get; set; }

            /// <summary> ログイン画面のURL </summary>
            public string LoginUrl { get; set; }

            /// <summary> 再起動設定のURL </summary>
            public string RestartSettingUrl { get; set; }

            /// <summary> パスワード入力BOXのID </summary>
            public string PasswordBoxId { get; set; }

            /// <summary> ログインボタンのID </summary>
            public string LoginButtonId { get; set; }

            /// <summary> 再起動設定画面のボタンフォームクラス名 </summary>
            public string RestartFormButtonsClassName { get; set; }

            /// <summary> 再起動設定画面のボタンフォームクラス内の再起動ボタンクラス名 </summary>
            public string RestartFormButtonsInRestartButtonClassName { get; set; }

            /// <summary> 再起動確認ボタンのID </summary>
            public string RestartApplyButtonId { get; set; }

            /// <summary> パスワード </summary>
            public string Password { get; set; }
        }

        public static void Main(string[] args)
        {
            IWebDriver webDriver;
            if (args.Length > 0)
            {
                webDriver = new ChromeDriver(args[0]);
            }
            else
            {
                webDriver = new ChromeDriver();
            }

            AutoRestartConfig config = new AutoRestartConfig();

            using (StreamReader reader = new StreamReader("Settings/AccessInfo.conf"))
            {
                string confStr = reader.ReadToEnd().Replace(" ", "");
                string[] settings = confStr.Split(separator: Environment.NewLine);

                foreach (var conf in settings)
                {
                    string[] keyValueConf = conf.Split(separator: "=");

                    switch (keyValueConf[0])
                    {
                        case "HOME_URL": 
                            config.HomeUrl = keyValueConf[1];
                            break;
                        case "LOGIN_URL":
                            config.LoginUrl = keyValueConf[1];
                            break;
                        case "RESTART_URL":
                            config.RestartSettingUrl = keyValueConf[1];
                            break;
                        case "PASSWORD_BOX_ID":
                            config.PasswordBoxId = keyValueConf[1];
                            break;
                        case "LOGIN_BUTTON_ID":
                            config.LoginButtonId = keyValueConf[1];
                            break;
                        case "RESTART_FORM_CLASS_NAME":
                            config.RestartFormButtonsClassName = keyValueConf[1];
                            break;
                        case "RESTART_BUTTON_CLASS_NAME":
                            config.RestartFormButtonsInRestartButtonClassName = keyValueConf[1];
                            break;
                        case "RESTART_APPLY_BUTTON_ID":
                            config.RestartApplyButtonId = keyValueConf[1];
                            break;
                        case "PASSWORD":
                            config.Password = keyValueConf[1];
                            break;
                        default:
                            break;
                    }
                }
            }

            try
            {
                // ログイン画面に移動
                webDriver.Navigate().GoToUrl(config.LoginUrl);
                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

                // パスワード入力
                var passwordBox = webDriver.FindElement(By.Id("txtPwd"));
                passwordBox.SendKeys("hzE84ghnDrkZ");

                // ログイン
                var loginButton = webDriver.FindElement(By.Id("btnLogin"));
                loginButton.Click();

                // ログイン出来ているかをホームに移動できたかで確認
                try
                {
                    WebDriverWait driverWait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
                    driverWait.Until(d => webDriver.Url == config.HomeUrl);
                }
                catch
                {
                    webDriver.Quit();
                    return;
                }

                // 早すぎると移動できないので、500ms待機させる
                Thread.Sleep(500);

                // 再起動設定に移動
                webDriver.Navigate().GoToUrl(config.RestartSettingUrl);
                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

                // 再起動ボタンを探す
                var formButtons = webDriver.FindElements(By.ClassName("form-buttons"));
                if (formButtons.Count > 0)
                {
                    // 再起動ボタンをぽち
                    var restartButton = formButtons[0].FindElement(By.ClassName("btn-primary"));
                    restartButton.Click();

                    // 再起動確認ポップアップではいを押す
                    var restartApplyButton = webDriver.FindElement(By.Id(config.RestartApplyButtonId));
                    restartApplyButton.Click();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            // ドライバの破棄
            webDriver.Quit();

            // 終了
            Environment.Exit(0);
        }
    }
}