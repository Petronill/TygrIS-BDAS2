﻿#pragma checksum "..\..\..\UserProfile.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "051C63EDCDD254FB6C37FBF8D50E2BD48D606F10"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using TISWindows;


namespace TISWindows {
    
    
    /// <summary>
    /// UserProfile
    /// </summary>
    public partial class UserProfile : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 20 "..\..\..\UserProfile.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Zoo;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\UserProfile.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Animals;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\UserProfile.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Info;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\UserProfile.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Login;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.1.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/TISWindows;V1.0.0.0;component/userprofile.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\UserProfile.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "7.0.1.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.Zoo = ((System.Windows.Controls.Button)(target));
            
            #line 20 "..\..\..\UserProfile.xaml"
            this.Zoo.Click += new System.Windows.RoutedEventHandler(this.OnClickZoo);
            
            #line default
            #line hidden
            return;
            case 2:
            this.Animals = ((System.Windows.Controls.Button)(target));
            
            #line 21 "..\..\..\UserProfile.xaml"
            this.Animals.Click += new System.Windows.RoutedEventHandler(this.OnClickAnimals);
            
            #line default
            #line hidden
            return;
            case 3:
            this.Info = ((System.Windows.Controls.Button)(target));
            
            #line 22 "..\..\..\UserProfile.xaml"
            this.Info.Click += new System.Windows.RoutedEventHandler(this.OnClickInfo);
            
            #line default
            #line hidden
            return;
            case 4:
            this.Login = ((System.Windows.Controls.Button)(target));
            
            #line 23 "..\..\..\UserProfile.xaml"
            this.Login.Click += new System.Windows.RoutedEventHandler(this.OnClickLogin);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

