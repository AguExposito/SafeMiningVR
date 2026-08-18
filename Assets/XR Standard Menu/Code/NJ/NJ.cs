using UnityEngine;
using System;
using System.Collections.Generic;

namespace NJ
{
    namespace Observable
    {
        public class Observable<T>
        {
            protected T _value;
            public event Action<T> OnValueChanged;
            public event Action<T, T> OnValueChangedWithPrevious;

            public T Value
            {
                get => _value;
                set
                {
                    if (!Equals(_value, value))
                    {
                        T previousValue = _value;
                        _value = value;

                        Debug.Log($"Valor cambiado de {previousValue} a {value}");

                        OnValueChanged?.Invoke(_value);
                        OnValueChangedWithPrevious?.Invoke(_value, previousValue);
                    }
                }
            }

            public Observable(T initialValue)
            {
                _value = initialValue;
            }

            public static implicit operator T(Observable<T> observable)
            {
                return observable._value;
            }
        }
    }
    namespace Finder
    {
        public class ComponentFinder
        {
            public static T FindComponentInScene<T>() where T : Component
            {
                T component = UnityEngine.Object.FindAnyObjectByType<T>();
                if (component != null)
                    return component;

                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

                foreach (GameObject obj in allObjects)
                {
                    if (obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave)
                        continue;

                    if (obj.scene.name == null)
                        continue;

                    T foundComponent = obj.GetComponent<T>();
                    if (foundComponent != null)
                        return foundComponent;
                }

                return null;
            }
            public static T FindComponentInSceneWithName<T>(string name) where T : Component
            {
                T component = UnityEngine.Object.FindAnyObjectByType<T>();
                if (component != null && component.gameObject != null && component.gameObject.name == name)
                    return component;

                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

                foreach (GameObject obj in allObjects)
                {
                    if (obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave)
                        continue;

                    // Ignorar objetos que no pertenecen a una escena válida (prefabs, etc.)
                    if (obj.scene.name == null)
                        continue;

                    if (obj.name != name)
                        continue;

                    T foundComponent = obj.GetComponent<T>();
                    if (foundComponent != null)
                        return foundComponent;
                }

                return null;
            }
            public static T[] FindAllComponentsInSceneWithName<T>(string name) where T : Component
            {
                List<T> components = new List<T>();
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

                foreach (GameObject obj in allObjects)
                {
                    if (obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave)
                        continue;

                    if (obj.scene.name == null)
                        continue;

                    if (obj.name != name)
                        continue;

                    T foundComponent = obj.GetComponent<T>();
                    if (foundComponent != null)
                        components.Add(foundComponent);
                }

                return components.ToArray();
            }
            public static T[] FindAllComponentsInScene<T>() where T : Component
            {
                List<T> components = new List<T>();
                GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

                foreach (GameObject obj in allObjects)
                {
                    if (obj.hideFlags == HideFlags.NotEditable || obj.hideFlags == HideFlags.HideAndDontSave)
                        continue;

                    if (obj.scene.name == null)
                        continue;

                    T foundComponent = obj.GetComponent<T>();
                    if (foundComponent != null)
                        components.Add(foundComponent);
                }

                return components.ToArray();
            }
        }
    }

}
