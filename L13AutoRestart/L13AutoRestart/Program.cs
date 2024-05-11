using System;
using System.IO;
using System.Net.NetworkInformation;

using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V122.Network;
using OpenQA.Selenium.Support.UI;

namespace L13AutoRestart
{
    class Program
    {
        private struct AutoRestartConfig
        {
            public AutoRestartConfig(string homeUrl, string loginUrl, string restartSettingUrl, string passwordBoxId, string loginButtonId, string restartFormButtonsClassName, string restartFormButtonsInRestartButtonClassName, string restartApplyButtonId, string password)
            {
                this.HomeUrl = homeUrl;
                this.LoginUrl = loginUrl;
                this.RestartSettingUrl = restartSettingUrl;
                this.PasswordBoxId = passwordBoxId;
                this.LoginButtonId = loginButtonId;
                this.RestartFormButtonsClassName = restartFormButtonsClassName;
                this.RestartFormButtonsInRestartButtonClassName = restartFormButtonsInRestartButtonClassName;
                this.RestartApplyButtonId = restartApplyButtonId;
                this.Password = password;
            }
            /// <summary> ホーム画面のURL </summary>
            public string HomeUrl { get; init; }

            /// <summary> ログイン画面のURL </summary>
            public string LoginUrl {  get; init; }

            /// <summary> 再起動設定のURL </summary>
            public string RestartSettingUrl { get; init; }

            /// <summary> パスワード入力BOXのID </summary>
            public string PasswordBoxId { get; init; }

            /// <summary> ログインボタンのID </summary>
            public string LoginButtonId { get; init; }

            /// <summary> 再起動設定画面のボタンフォームクラス名 </summary>
            public string RestartFormButtonsClassName { get; init; }

            /// <summary> 再起動設定画面のボタンフォームクラス内の再起動ボタンクラス名 </summary>
            public string RestartFormButtonsInRestartButtonClassName { get; init; }

            /// <summary> 再起動確認ボタンのID </summary>
            public string RestartApplyButtonId { get; init; }

            /// <summary> パスワード </summary>
            public string Password { get; init; }
        }

        public static void Main(string[] args)
        {
            // L13の再起動実行したか(1度実行すると5分間はtrueのまま)
            bool isExecuteRestartL13 = false;
            int executedTimerCount = 0;

            // 1分に1回タイマーでネットワーク接続の確認を実行する (1000 * 60) * 1
            System.Timers.Timer timer = new System.Timers.Timer(1000);
            timer.Elapsed += (sender, e) =>
            {
                if (isExecuteRestartL13)
                {
                    // 再起動実行後、5分以内は再度実行しない
                    if (executedTimerCount >= 50)
                    {
                        isExecuteRestartL13 = false;
                        executedTimerCount = 0;
                    }
                    else
                    {
                        executedTimerCount++;
                    }
                }
                else
                {
                    // wnkr.tech -> google.comの順にチェックし、どちらも応答なかったらネット接続なしと判断
                    if (!isEnabledInternetAccessForPing("wnkr.tech"))
                    {
                        Console.WriteLine("[wnkr.tech] ping error");
                        if (!isEnabledInternetAccessForPing("google.com"))
                        {
                            Console.WriteLine("[google.com] ping error");

                            // インターネットに接続できていなければL13を再起動する
                            isExecuteRestartL13 = true;
                            executedTimerCount = 0;
                            restartL13WiFiRouter();
                        }
                    }
                }
            };

            timer.Start();

            Console.ReadKey();
        }

        private static void restartL13WiFiRouter()
        {
            AutoRestartConfig config = new AutoRestartConfig(
                "empty(test data)",
                "empty(test data)",
                "empty(test data)",
                "empty(test data)",
                "empty(test data)",
                "empty(test data)",
                "empty(test data)",
                "empty(test data)",
                "empty(test data)"
            );

            IWebDriver webDriver = new ChromeDriver();

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
            catch
            {
                throw;
            }

            webDriver.Quit();
        }

        private static bool isEnabledInternetAccessForPing(string domainName)
        {
            bool isSuccess = false;

            try
            {
                var ping = new Ping();
                var replies = new List<PingReply>();

                for (int i = 0; i < 5; i++)
                {
                    var reply = ping.Send(domainName);
                    replies.Add(reply);
                }

                isSuccess = replies.Any(reply => reply.Status == IPStatus.Success);
            }
            catch
            {
                isSuccess = false;
            }

            return isSuccess;
        }
    }
}