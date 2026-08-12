using System;
using System.Globalization;

namespace ConfigCrate.EmptyFolderRadar
{
    internal sealed class LocaleText
    {
        public LocaleText() { IsChinese = CultureInfo.CurrentUICulture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase); }
        public bool IsChinese { get; private set; }
        public void Toggle() { IsChinese = !IsChinese; }
        private string T(string zh, string en) { return IsChinese ? zh : en; }
        public string Subtitle { get { return T("只找空文件夹，删除前让你看清楚。", "Find only empty folders. Review before cleanup."); } }
        public string Choose { get { return T("选择要检查的文件夹", "Choose a folder to inspect"); } }
        public string ChooseHint { get { return T("不会扫描整个电脑 · 不读取文件内容 · 不永久删除", "No whole-PC scan · No file contents read · No permanent deletion"); } }
        public string Browse { get { return T("选择文件夹", "Choose folder"); } }
        public string ScanAgain { get { return T("重新扫描", "Scan again"); } }
        public string Waiting { get { return T("等待选择文件夹", "Waiting for a folder"); } }
        public string Scanning { get { return T("正在扫描...", "Scanning..."); } }
        public string Found(int groups, int total) { return T("发现 " + groups + " 组空目录", groups + (groups == 1 ? " empty branch" : " empty branches")) + T(" · 共 " + total + " 个文件夹", " · " + total + " folders total"); }
        public string None { get { return T("没有发现空文件夹", "No empty folders found"); } }
        public string NoneHint { get { return T("这个目录不需要清理。", "This folder needs no cleanup."); } }
        public string SelectAll { get { return T("全选", "Select all"); } }
        public string SelectNone { get { return T("取消全选", "Select none"); } }
        public string Recycle { get { return T("移入回收站", "Move to Recycle Bin"); } }
        public string Open { get { return T("打开文件夹", "Open folder"); } }
        public string Folder { get { return T("空目录", "Empty folder"); } }
        public string Includes { get { return T("包含", "Includes"); } }
        public string Modified { get { return T("最后修改", "Last modified"); } }
        public string EmptyOnly { get { return T("空文件夹", "empty folder"); } }
        public string EmptyMany(int count) { return T("含 " + count + " 个空子目录", "contains " + count + " empty descendants"); }
        public string Confirm(int selected, int total) { return T("把选中的 " + selected + " 组空目录（共 " + total + " 个文件夹）移入回收站？\n\n不会永久删除，可以从Windows回收站恢复。", "Move " + selected + " selected empty branches (" + total + " folders total) to the Recycle Bin?\n\nNothing is permanently deleted and Windows can restore them."); }
        public string Done(int count) { return T("已把 " + count + " 组空目录移入回收站。", count + " empty branches moved to the Recycle Bin."); }
        public string Partial(int done, int failed) { return T("已处理 " + done + " 组，" + failed + " 组失败。请重新扫描查看。", done + " processed; " + failed + " failed. Scan again to review."); }
        public string Unsafe { get { return T("为了安全，不能直接扫描磁盘根目录、Windows、Program Files或整个用户主目录。请选择里面更具体的文件夹。", "For safety, drive roots, Windows, Program Files, and the whole user profile cannot be scanned directly. Choose a more specific folder inside them."); } }
        public string Local { get { return T("本地扫描 · 删除只进回收站", "Local scan · Recycle Bin only"); } }
        public string Error { get { return T("无法完成", "Could not complete"); } }
        public string Warnings(int count) { return T("另有 " + count + " 个目录无法访问", count + " inaccessible folders skipped"); }
    }
}
