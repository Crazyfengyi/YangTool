#if UNITY_EDITOR
using System;
using System.Net;
using System.Net.Mail;
using System.Linq;
using System.Threading.Tasks;

namespace YangTools.AutoBuildSystem
{
    /// <summary>
    /// 通过 SMTP 发送自动构建通知
    /// </summary>
    internal static class AutoBuildNotifier
    {
        /// <summary>
        /// 发送构建完成或失败邮件
        /// </summary>
        public static async Task SendCompletionAsync(AutoBuildProfile profile, AutoBuildBuildRecord record)
        {
            string subject = $"[{record.status}] {record.profileName} {record.finishedAt}";
            string body = $"档案：{record.profileName}\r\n" +
                          $"结果：{record.status}\r\n" +
                          $"开始：{record.startedAt}\r\n" +
                          $"结束：{record.finishedAt}\r\n" +
                          $"耗时：{record.durationSeconds:0.0} 秒\r\n" +
                          $"说明：{record.message}\r\n" +
                          $"产物：{string.Join("\r\n", record.outputPaths)}\r\n" +
                          $"日志摘要：\r\n{ReadLogSummary(record.logPath)}\r\n" +
                          $"日志：{record.logPath}";
            await SendAsync(profile, subject, body);
        }

        /// <summary>
        /// 发送测试邮件以验证 SMTP 配置
        /// </summary>
        public static Task SendTestAsync(AutoBuildProfile profile)
        {
            return SendAsync(profile, "YangTools 自动构建邮件测试", "SMTP 配置测试成功。此邮件不包含任何构建凭据。");
        }

        /// <summary>
        /// 以档案的 SMTP 参数异步发送邮件
        /// </summary>
        private static async Task SendAsync(AutoBuildProfile profile, string subject, string body)
        {
            if (profile?.email == null || !profile.email.enabled)
            {
                throw new InvalidOperationException("该构建档案未启用邮件通知");
            }

            string password = AutoBuildSecretStore.Get(AutoBuildCredentialKeys.ForProfile(profile, "smtp.password"));
            if (string.IsNullOrEmpty(password)) throw new InvalidOperationException("SMTP 密码尚未保存在本机凭据中");

            using (MailMessage message = new MailMessage())
            using (SmtpClient client = new SmtpClient(profile.email.host, profile.email.port))
            {
                message.From = new MailAddress(profile.email.sender);
                foreach (string address in profile.email.recipient.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    message.To.Add(address.Trim());
                }

                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = false;
                client.EnableSsl = profile.email.enableTls;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Timeout = 20000;
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(profile.email.username, password);
                await client.SendMailAsync(message);
            }
        }

        /// <summary>
        /// 读取构建日志末尾作为通知摘要
        /// </summary>
        private static string ReadLogSummary(string logPath)
        {
            if (string.IsNullOrWhiteSpace(logPath) || !System.IO.File.Exists(logPath)) return "日志文件不存在";
            string[] lines = System.IO.File.ReadAllLines(logPath);
            int start = Math.Max(0, lines.Length - 20);
            string summary = string.Join("\r\n", lines.Skip(start));
            return summary.Length > 3000 ? summary.Substring(summary.Length - 3000) : summary;
        }
    }
}
#endif
