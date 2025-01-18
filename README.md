# WiX installer with WPF interface

This repository contains a WPF application that takes the place of the WiX dialog sets for MSI based installers.  The built in MSI dialogs are silent and the WPF application guides the user through the installation process.  
The idea behind this project is to make it easier than using MSI dialogs for installer configuration.

##### Features:

 * Dependency injection using Microsoft.Extensions.DependencyInjection
 * Styling & controls courtesy of MahApps Metro
 * MVVM (CommunityToolkit.Mvvm)