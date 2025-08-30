using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace UniGame.Localization.Runtime
{
    using System.Threading;
    using Core.Runtime;
    using R3;
    using TMPro;
    using UnityEngine;

    public static class LocalizationExtensions
    {
        public static void SetValue(
            this TextMeshProUGUI value,
            LocalizedString localizedString, 
            ILifeTime lifeTime)
        {
            if(value == null) return;
            if(localizedString == null) return;
            
            localizedString.AsObservable()
                .Subscribe(value, (x, y) => y.text = x)
                .AddTo(lifeTime);
        }
        
        public static async UniTask<string> SetValueAsync(
            this TextMeshProUGUI value,
            LocalizedString localizedString, 
            CancellationToken token = default)
        {
            if (value == null) return string.Empty;
            
            var stringTask = localizedString
                .GetLocalizedStringAsync();
            
            await stringTask.ToUniTask(cancellationToken: token);
            var stringValue = stringTask.Result;
            
            if (value == null) return string.Empty;
            value.text = stringValue;
            
            return stringValue;
        }
        
        public static LocalizedString ToLocalizedString(this string key)
        {
            var splittedKey = key.Split('/');
            if (splittedKey.Length < 2)
                return null;

            var table = splittedKey[0];
            var entry = splittedKey[1];

            var localizedString = new LocalizedString();
            localizedString.SetReference(table, entry);

            return localizedString;
        }

        public static LocalizedString ToLocalizedString(this TableEntryReference reference)
        {
            var defaultTable = LocalizationSettings.StringDatabase.DefaultTable;
            return ToLocalizedString(reference, defaultTable);
        }

        public static LocalizedString ToLocalizedString(this TableEntryReference reference, TableReference tableEntry)
        {
            var result = new LocalizedString();
            result.SetReference(tableEntry, reference);
            return result;
        }
        
        public static IDisposable Subscribe(this LocalizedString source, IObserver<string> handler,int frameThrottle = 1)
        {
            return Subscribe(source,x => handler?.OnNext(x),frameThrottle);          
        }

        public static IDisposable Subscribe(this LocalizedString source, Action<string> text, int frameThrottle = 1)
        {
            if (source == null || text == null) return Disposable.Empty;
            
            var result = Observable
                .Create<string>(x => Bind(source, x, frameThrottle),true)
                .Do(x => text?.Invoke(x))
                .Subscribe();
            
            return result;
        }

        
        public static IDisposable Bind(this LocalizedString source, Action<string> text, int frameThrottle = 1)
        {
            if (source == null || text == null) return Disposable.Empty;
            
            var result = Observable
                .Create<string>(x => Bind(source, x, frameThrottle),true)
                .Do(text.Invoke)
                .Subscribe();
            
            return result;
        }

        
        public static IDisposable Bind(this LocalizedString source, ReactiveProperty<string> text, int frameThrottle = 1)
        {
            if (text == null) return Disposable.Empty;
            
            var result = Observable
                .Create<string>(x => Bind(source, x, frameThrottle),true)
                .Do(x => text.Value = x)
                .Subscribe();
            
            return result;
        }
        
        public static IDisposable Bind(this LocalizedString source, IObserver<string> action,int frameThrottle = 1)
        {
            return Bind(source,action.ToObserver(),frameThrottle);       
        }
        
        public static IDisposable Bind(this LocalizedString source, Observer<string> action,int frameThrottle = 1)
        {
            if(source == null || action == null)
                return Disposable.Empty;

            var result = source
                .AsObservable()
                .DelayFrame(frameThrottle)
                .Subscribe(action);

            source.RefreshString();
            
            return result;          
        }

        public static Observable<string> AsObservable(this LocalizedString localizedString)
        {
            return Observable.FromEvent<string>(y  => localizedString.StringChanged += y.Invoke,
                y => localizedString.StringChanged -= y.Invoke);
        }
        
        public static Observable<TAsset> AsObservable<TAsset>(this LocalizedAsset<TAsset> localizedAsset) 
            where TAsset : Object
        {
            return Observable.FromEvent<TAsset>(y  => localizedAsset.AssetChanged += y.Invoke,
                y => localizedAsset.AssetChanged -= y.Invoke);
        }

    }
}
