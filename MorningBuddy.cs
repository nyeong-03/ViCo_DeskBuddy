using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MorningBuddy {
  class Reminder {
    public string Title;
    public DateTime When;
    public int Before;
    public bool Notified;
    public override string ToString() { return When.ToString("MM/dd  HH:mm") + "   " + Title + "   · " + Before + "분 전 알림"; }
  }
  static class Program {
    public static string DataFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "morning-reminders.txt");
    public static List<Reminder> Events = new List<Reminder>();
    [STAThread] static void Main() {
      Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
      LoadEvents(); Application.Run(new MainWindow());
    }
    public static void LoadEvents() {
      if (!File.Exists(DataFile)) return;
      try { foreach (string line in File.ReadAllLines(DataFile, Encoding.UTF8)) {
        string[] p = line.Split('|'); if (p.Length != 4) continue;
        Events.Add(new Reminder { Title=Encoding.UTF8.GetString(Convert.FromBase64String(p[0])), When=DateTime.FromBinary(long.Parse(p[1])), Before=int.Parse(p[2]), Notified=bool.Parse(p[3]) });
      }} catch { Events = new List<Reminder>(); }
    }
    public static void SaveEvents() {
      try { List<string> lines = new List<string>(); foreach (Reminder e in Events) lines.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes(e.Title))+"|"+e.When.ToBinary()+"|"+e.Before+"|"+e.Notified); File.WriteAllLines(DataFile, lines.ToArray(), Encoding.UTF8); } catch {}
    }
  }
  static class UiScale {
    public static void FitFonts(Control root) {
      float dpi=96f; try { using(Graphics g=Graphics.FromHwnd(IntPtr.Zero)) dpi=g.DpiX; } catch {}
      float factor=96f/dpi; Fit(root,factor);
    }
    static void Fit(Control root,float factor) {
      foreach(Control c in root.Controls) {
        if(c.Font!=null) c.Font=new Font(c.Font.FontFamily,Math.Max(6f,c.Font.Size*factor),c.Font.Style,c.Font.Unit);
        if(c.HasChildren) Fit(c,factor);
      }
    }
  }
  class MainWindow : Form {
    Label bubble; ListBox list; NotifyIcon tray; Timer timer;
    readonly Color ink=Color.FromArgb(48,44,42), cream=Color.FromArgb(255,250,247), yellow=Color.FromArgb(255,224,93);
    public MainWindow() {
      Text="모닝 - 데스크 비서"; Size=new Size(280,425); MinimumSize=MaximumSize=Size; FormBorderStyle=FormBorderStyle.None; StartPosition=FormStartPosition.Manual; TopMost=true; BackColor=cream; ShowInTaskbar=true;
      Rectangle work=Screen.PrimaryScreen.WorkingArea; Location=new Point(Math.Max(0,work.Right-300),Math.Max(0,work.Bottom-450));
      Panel header=new Panel { Location=new Point(0,0), Size=new Size(280,44), BackColor=Color.FromArgb(255,244,224) }; Controls.Add(header);
      Label brand=new Label { Text="모닝", Location=new Point(14,7), Size=new Size(72,31), Font=new Font("Malgun Gothic",16,FontStyle.Bold), ForeColor=ink, BackColor=header.BackColor }; header.Controls.Add(brand);
      Label subtitle=new Label { Text="DESK BUDDY", Location=new Point(87,15), Size=new Size(112,17), Font=new Font("Malgun Gothic",7), ForeColor=Color.FromArgb(151,139,132), BackColor=header.BackColor }; header.Controls.Add(subtitle);
      Button close=new Button { Text="×", Location=new Point(248,7), Size=new Size(25,26), FlatStyle=FlatStyle.Flat, BackColor=header.BackColor, ForeColor=Color.FromArgb(94,83,77), Font=new Font("Malgun Gothic",13) }; close.FlatAppearance.BorderSize=0; close.Click+=(s,e)=>Close(); header.Controls.Add(close);
      header.MouseDown+=Drag; brand.MouseDown+=Drag; subtitle.MouseDown+=Drag;
      Panel smile=new Panel { Location=new Point(108,50), Size=new Size(64,64), BackColor=cream }; smile.Paint+=DrawSmile; Controls.Add(smile);
      Controls.Add(MakeLabel("오늘도 같이 챙겨볼게요",15,115,250,24,12,ink,true,ContentAlignment.MiddleCenter));
      Controls.Add(MakeLabel("●  함께 있는 중",78,139,124,17,8,Color.FromArgb(119,161,123),false,ContentAlignment.MiddleCenter));
      bubble=new Label { Text="안녕! 일정과 할 일을 같이 챙겨보자.", Location=new Point(16,162), Size=new Size(248,38), Font=new Font("Malgun Gothic",9), ForeColor=Color.FromArgb(97,86,87), BackColor=Color.White, BorderStyle=BorderStyle.FixedSingle, TextAlign=ContentAlignment.MiddleCenter }; Controls.Add(bubble);
      Controls.Add(MakeLabel("다가오는 일정",16,210,210,22,10,ink,true,ContentAlignment.MiddleLeft));
      list=new ListBox { Location=new Point(16,235), Size=new Size(248,94), BorderStyle=BorderStyle.None, BackColor=Color.White, Font=new Font("Malgun Gothic",8), ForeColor=Color.FromArgb(77,68,68), HorizontalScrollbar=true }; Controls.Add(list);
      Button add=new Button { Text="＋ 추가", Location=new Point(16,342), Size=new Size(91,36), FlatStyle=FlatStyle.Flat, BackColor=yellow, ForeColor=ink, Font=new Font("Malgun Gothic",9,FontStyle.Bold) }; add.FlatAppearance.BorderSize=0; add.Click+=AddReminder; Controls.Add(add);
      Button edit=new Button { Text="수정", Location=new Point(113,342), Size=new Size(68,36), FlatStyle=FlatStyle.Flat, BackColor=Color.White, ForeColor=Color.FromArgb(100,87,84), Font=new Font("Malgun Gothic",8) }; edit.FlatAppearance.BorderColor=Color.FromArgb(235,224,218); edit.Click+=EditReminder; Controls.Add(edit);
      Button remove=new Button { Text="삭제", Location=new Point(187,342), Size=new Size(77,36), FlatStyle=FlatStyle.Flat, BackColor=Color.White, ForeColor=Color.FromArgb(100,87,84), Font=new Font("Malgun Gothic",8) }; remove.FlatAppearance.BorderColor=Color.FromArgb(235,224,218); remove.Click+=RemoveReminder; Controls.Add(remove);
      list.DoubleClick+=EditReminder;
      Controls.Add(MakeLabel("이 컴퓨터에 저장됨 · 서비스 연결은 다음 단계",12,392,256,17,7,Color.FromArgb(163,153,151),false,ContentAlignment.MiddleCenter));
      tray=new NotifyIcon { Icon=SystemIcons.Information, Visible=true, Text="모닝 - 일정 알림" }; tray.DoubleClick+=(s,e)=>{Show();WindowState=FormWindowState.Normal;Activate();};
      timer=new Timer { Interval=5000 }; timer.Tick+=CheckReminders; timer.Start(); FormClosed+=(s,e)=>{timer.Stop();tray.Visible=false;tray.Dispose();}; RefreshList(); UiScale.FitFonts(this);
    }
    Label MakeLabel(string text,int x,int y,int w,int h,float size,Color color,bool bold,ContentAlignment align) { return new Label { Text=text, Location=new Point(x,y), Size=new Size(w,h), Font=new Font("Malgun Gothic",size,bold?FontStyle.Bold:FontStyle.Regular), ForeColor=color, BackColor=BackColor, TextAlign=align }; }
    void DrawSmile(object sender,PaintEventArgs e) {
      const int factor=3; int w=smileSize(sender); int h=w;
      using(Bitmap art=new Bitmap(w*factor,h*factor)) {
        using(Graphics g=Graphics.FromImage(art)) {
          g.SmoothingMode=SmoothingMode.HighQuality;g.PixelOffsetMode=PixelOffsetMode.HighQuality;g.ScaleTransform(factor,factor);
          using(SolidBrush face=new SolidBrush(yellow)) using(Pen outline=new Pen(ink,1.8f)) using(SolidBrush eyes=new SolidBrush(ink)) using(GraphicsPath mouth=new GraphicsPath()) {
            outline.LineJoin=LineJoin.Round;g.FillEllipse(face,3,3,w-6,h-6);g.DrawEllipse(outline,3,3,w-6,h-6);
            g.FillEllipse(eyes,w*.34f,h*.35f,3.2f,4.2f);g.FillEllipse(eyes,w*.62f,h*.35f,3.2f,4.2f);
            mouth.AddBezier(w*.29f,h*.56f,w*.37f,h*.78f,w*.63f,h*.78f,w*.71f,h*.56f);g.DrawPath(outline,mouth);
          }
        }
        e.Graphics.InterpolationMode=InterpolationMode.HighQualityBicubic;e.Graphics.SmoothingMode=SmoothingMode.HighQuality;e.Graphics.DrawImage(art,new Rectangle(0,0,w,h));
      }
    }
    int smileSize(object sender){return ((Control)sender).ClientSize.Width;}
    void Drag(object s,MouseEventArgs e) { if(e.Button==MouseButtons.Left) { Native.ReleaseCapture();Native.SendMessage(Handle,0xA1,(IntPtr)2,IntPtr.Zero); } }
    void RefreshList() {
      list.Items.Clear(); List<Reminder> upcoming=Program.Events.FindAll(x=>x.When>DateTime.Now);upcoming.Sort((a,b)=>a.When.CompareTo(b.When));
      if(upcoming.Count==0)list.Items.Add("등록된 일정이 없어요. + 일정 추가를 눌러보세요.");else for(int i=0;i<Math.Min(8,upcoming.Count);i++)list.Items.Add(upcoming[i]);
      if(upcoming.Count>0)bubble.Text="다음 일정은 "+upcoming[0].When.ToString("HH:mm")+" · "+upcoming[0].Title;else bubble.Text="안녕! 일정과 할 일을 같이 챙겨보자. 아래에서 일정을 추가해봐.";
    }
    void AddReminder(object s,EventArgs e) {
      using(AddWindow dialog=new AddWindow(null)) if(dialog.ShowDialog(this)==DialogResult.OK) {Program.Events.Add(dialog.Result);Program.SaveEvents();RefreshList();}
    }
    void EditReminder(object s,EventArgs e) {
      Reminder selected=list.SelectedItem as Reminder;
      if(selected==null){MessageBox.Show(this,"먼저 수정할 일정을 목록에서 선택해줘.","모닝");return;}
      using(AddWindow dialog=new AddWindow(selected)) if(dialog.ShowDialog(this)==DialogResult.OK) {selected.Title=dialog.Result.Title;selected.When=dialog.Result.When;selected.Before=dialog.Result.Before;selected.Notified=false;Program.SaveEvents();RefreshList();}
    }
    void RemoveReminder(object s,EventArgs e) { Reminder selected=list.SelectedItem as Reminder;if(selected!=null){Program.Events.Remove(selected);Program.SaveEvents();RefreshList();} }
    void CheckReminders(object s,EventArgs e) {
      DateTime now=DateTime.Now;bool changed=false;
      foreach(Reminder r in Program.Events) if(!r.Notified) { DateTime due=r.When.AddMinutes(-r.Before);if(now>=due&&now<r.When){tray.BalloonTipTitle="곧 시작해요";tray.BalloonTipText=r.Title+" · "+r.Before+"분 전 알림";tray.BalloonTipIcon=ToolTipIcon.Info;tray.ShowBalloonTip(10000);r.Notified=true;changed=true;}else if(now>=r.When){r.Notified=true;changed=true;} }
      if(changed){Program.SaveEvents();RefreshList();}
    }
    protected override void Dispose(bool disposing){if(disposing&&components!=null)components.Dispose();base.Dispose(disposing);}
    System.ComponentModel.IContainer components;
  }
  class AddWindow : Form {
    public Reminder Result;
    TextBox title;DateTimePicker date;ComboBox hour,minute,before;
    public AddWindow(Reminder existing){Text=existing==null?"일정 추가":"일정 수정";Size=new Size(340,320);FormBorderStyle=FormBorderStyle.FixedDialog;StartPosition=FormStartPosition.CenterParent;MaximizeBox=false;MinimizeBox=false;BackColor=Color.FromArgb(255,250,247);
      Controls.Add(new Label{Text=existing==null?"무엇을 기억할까요?":"일정 내용을 수정해줘",Location=new Point(18,14),Size=new Size(290,23),Font=new Font("Malgun Gothic",11,FontStyle.Bold),ForeColor=Color.FromArgb(48,44,42)});
      title=new TextBox{Location=new Point(18,43),Size=new Size(285,26),Font=new Font("Malgun Gothic",9)};Controls.Add(title);
      Controls.Add(new Label{Text="날짜",Location=new Point(18,81),Size=new Size(80,18),Font=new Font("Malgun Gothic",8),ForeColor=Color.FromArgb(117,105,106)});
      DateTime initial=DateTime.Now.AddHours(1);initial=new DateTime(initial.Year,initial.Month,initial.Day,initial.Hour,initial.Minute,0);
      date=new DateTimePicker{Location=new Point(18,101),Size=new Size(285,26),Format=DateTimePickerFormat.Short,Value=initial.Date};Controls.Add(date);
      Controls.Add(new Label{Text="시간",Location=new Point(18,137),Size=new Size(60,18),Font=new Font("Malgun Gothic",8),ForeColor=Color.FromArgb(117,105,106)});
      hour=new ComboBox{Location=new Point(18,157),Size=new Size(68,25),DropDownStyle=ComboBoxStyle.DropDownList,Font=new Font("Malgun Gothic",9)};for(int i=0;i<24;i++)hour.Items.Add(i.ToString("00"));hour.SelectedItem=initial.Hour.ToString("00");Controls.Add(hour);
      Controls.Add(new Label{Text=":",Location=new Point(88,159),Size=new Size(12,22),Font=new Font("Malgun Gothic",10),ForeColor=Color.FromArgb(70,62,62)});
      minute=new ComboBox{Location=new Point(101,157),Size=new Size(68,25),DropDownStyle=ComboBoxStyle.DropDownList,Font=new Font("Malgun Gothic",9),MaxDropDownItems=12};for(int i=0;i<60;i++)minute.Items.Add(i.ToString("00"));minute.SelectedItem=initial.Minute.ToString("00");Controls.Add(minute);
      Controls.Add(new Label{Text="몇 분 전에 알려줄까요?",Location=new Point(18,198),Size=new Size(180,18),Font=new Font("Malgun Gothic",8),ForeColor=Color.FromArgb(117,105,106)});
      before=new ComboBox{Location=new Point(18,218),Size=new Size(105,25),DropDownStyle=ComboBoxStyle.DropDownList,Font=new Font("Malgun Gothic",9)};before.Items.AddRange(new object[]{"5분 전","10분 전","15분 전","30분 전","60분 전"});before.SelectedIndex=1;Controls.Add(before);
      Button cancel=new Button{Text="취소",Location=new Point(154,260),Size=new Size(68,32),BackColor=Color.White,FlatStyle=FlatStyle.Flat,Font=new Font("Malgun Gothic",8)};cancel.DialogResult=DialogResult.Cancel;Controls.Add(cancel);
      if(existing!=null){title.Text=existing.Title;date.Value=existing.When.Date;hour.SelectedItem=existing.When.Hour.ToString("00");minute.SelectedItem=existing.When.Minute.ToString("00");int[] choices={5,10,15,30,60};before.SelectedIndex=Math.Max(0,Array.IndexOf(choices,existing.Before));}
      Button save=new Button{Text="저장",Location=new Point(230,260),Size=new Size(73,32),BackColor=Color.FromArgb(255,224,93),FlatStyle=FlatStyle.Flat,Font=new Font("Malgun Gothic",9,FontStyle.Bold)};save.FlatAppearance.BorderSize=0;save.Click+=(s,e)=>{if(String.IsNullOrWhiteSpace(title.Text)){MessageBox.Show(this,"일정 이름을 적어줘.","모닝");return;}DateTime chosen=date.Value.Date.AddHours(int.Parse(hour.SelectedItem.ToString())).AddMinutes(int.Parse(minute.SelectedItem.ToString()));if(chosen<=DateTime.Now){MessageBox.Show(this,"현재보다 뒤의 시간으로 선택해줘.","모닝");return;}int[] minutes={5,10,15,30,60};Result=new Reminder{Title=title.Text.Trim(),When=chosen,Before=minutes[before.SelectedIndex],Notified=false};DialogResult=DialogResult.OK;};Controls.Add(save);AcceptButton=save;CancelButton=cancel;UiScale.FitFonts(this);
    }
  }
  static class Native { [System.Runtime.InteropServices.DllImport("user32.dll")] public static extern bool ReleaseCapture();[System.Runtime.InteropServices.DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr hWnd,int msg,IntPtr wParam,IntPtr lParam); }
}
