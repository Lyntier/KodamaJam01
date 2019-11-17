using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using UnityEditor;
using UnityEngine;

public class CleanRegistryKey
{
    [MenuItem("Registry/Clean Unity Build registry items")]
    static void Clean()
    {
        string keyPath = String.Format(
            "SOFTWARE\\{0}\\{1}",
            PlayerSettings.companyName,
            PlayerSettings.productName
        );

        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath, true))
        {
            if (key == null) // Key doesn't exist
            {
                Debug.LogWarning("Could not find key. Have you built this project at least once?\n"
                                 + "Key tried: " + keyPath);
                return;
            }

            List<string> screenManagerVals = key.GetValueNames()
                .Where(x => x.StartsWith("Screenmanager"))
                .ToList();

            if (!screenManagerVals.Any())
            {
                Debug.LogWarning("There were no 'Screenmanager' values in the registry. " +
                                 "Have you cleaned this key before?\n" +
                                 "Key tried: " + keyPath);
                return;
            }

            foreach (string value in screenManagerVals)
            {
                // These keys all have to do with setting screen size / type.
                Debug.Log("Deleting value: " + value);
                key.DeleteValue(value);
            }
            
            Debug.Log("Successfully deleted the 'Screenmanager' values.");
        }
    }
}