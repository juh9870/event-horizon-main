﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GameDatabase.Model;
using GameServices.Settings;
using Services.Resources;
using Zenject;
using Services.GameApplication;

namespace Services.Audio
{
	public class SoundPlayer : MonoBehaviour, ISoundPlayer
	{
	    [Inject]
	    private void Initialize(
			GameSettings gameSettings, 
			IResourceLocator resourceLocator,
			//SceneBeforeUnloadSignal sceneBeforeUnloadSignal,
			AppActivatedSignal appActivatedSignal)
	    {
            _appActivatedSignal = appActivatedSignal;
            _appActivatedSignal.Event += OnAppActivated;
            _gameSettings = gameSettings;
	        _resourceLocator = resourceLocator;
            Volume = _gameSettings.SoundVolume;
		
			//_sceneBeforeUnloadSignal = sceneBeforeUnloadSignal;
			//_sceneBeforeUnloadSignal.Event += OnSceneBeforeUnload;
		}

		public float Volume
		{
			get
			{
				return _volume;
			}
			set
			{
				_volume = value;
                _gameSettings.SoundVolume = _volume;
				_audioSources = GetComponents<AudioSource>().Select(item => new AudioSourceData { audioSource = item, id = 0 } ).ToList();
				foreach (var data in _audioSources)
					data.audioSource.volume = _volume;
			}
		}

        public void Play(AudioClipId audioClip, int soundId = 0)
        {
            if (Volume <= 0f)
                return;

            Enqueue(new AudioData { AudioClip = _resourceLocator.GetAudioClip(audioClip), Loop = audioClip.Loop, Id = soundId });
        }

        public void Play(AudioClip audioClip, int soundId = 0, bool loop = false)
		{
		    if (Volume <= 0f)
		        return;

		    Enqueue(new AudioData { AudioClip = audioClip, Loop = loop, Id = soundId });
		}

		public void Stop(int soundId)
		{
			Enqueue(new AudioData { AudioClip = null, Id = soundId });
		}

        public void OnAppActivated(bool active)
        {
            foreach (var data in _audioSources)
                if (active)
                    data.audioSource.UnPause();
	            else
		            data.audioSource.Pause();
		}

		private void OnSceneBeforeUnload()
		{
			foreach (var item in _audioSources)
				item.audioSource.Stop();

			while (Dequeue() != null);
		}

		private AudioSourceData FindAudioSource(int audioClipId)
		{
			foreach (var item in _audioSources)
				if (item.id == audioClipId) return item;

			return null;
		}

		private void StopImmediately(int id)
		{
			var audioSource = FindAudioSource(id);
			if (audioSource != null)
			{
				audioSource.id = 0;
				audioSource.audioSource.Stop();
				return;
			}

			lock (_lockObject)
			{
				foreach (var item in _objects)
					if (item.Id == id)
						item.AudioClip = null;
			}
		}

		private bool IsPlaying(int id)
		{
			var audioSource = FindAudioSource(id);
			return audioSource != null && audioSource.audioSource.isPlaying;
		}

		private void Update()
		{
			AudioData data;
			while ((data = Dequeue()) != null)
			{
				if (data.AudioClip == null && data.Id != 0)
				{
					StopImmediately(data.Id);
					continue;
				}

				if (data.Id != 0 && IsPlaying(data.Id))
					continue;
				
				if (data.AudioClip.loadState == AudioDataLoadState.Loading)
				{
                    _loadingObjects.Add(data);
					continue;
				}

				AudioSourceData audioSource;
				if (data.Id != 0)
					audioSource = _audioSources.FirstOrDefault(item => item.id == data.Id) ?? GetAudioSource();
				else
					audioSource = GetAudioSource();

	            if (audioSource == null)
	                continue;

				audioSource.audioSource.clip = data.AudioClip;
				audioSource.audioSource.loop = data.Loop;
				audioSource.audioSource.Play();
				audioSource.id = data.Id;
	        }

		    _timeFromLastLoadAttempt += Time.unscaledDeltaTime;
		    if (_timeFromLastLoadAttempt > 0.2f)
		    {
		        _timeFromLastLoadAttempt = 0.0f;

                lock (_lockObject)
                {
                    foreach (var item in _loadingObjects)
                    {
                        if (++item.LoadAttempts < 10)
                            _objects.Enqueue(item);
                    }
                }

                _loadingObjects.Clear();
            }
        }

		private AudioSourceData GetAudioSource()
		{
			var audioSource = _audioSources.FirstOrDefault(item => !item.audioSource.isPlaying);
			if (audioSource == null)
			{
				var min = int.MaxValue;
				foreach (var item in _audioSources)
				{
					if (item.audioSource.loop)
						continue;

					var value = item.audioSource.clip.samples - item.audioSource.timeSamples;
					if (value < min)
					{
						audioSource = item;
						min = value;
					}
				}
			}

			return audioSource;
		}

		private void Enqueue(AudioData data)
		{
			lock (_lockObject)
			{
			    if (data.AudioClip != null)
			    {
                    if (TotalClipsInQueue(data.AudioClip) > 2)
                        return;

			        AddClipToQueue(data.AudioClip);
			    }

			    _objects.Enqueue(data);
			}
		}
		
		private AudioData Dequeue()
		{
			lock (_lockObject)
			{
                var data = _objects.Count > 0 ? _objects.Dequeue() : null;

                if (data != null && data.AudioClip != null)
                    RemoveClipFromQueue(data.AudioClip);

			    return data;
			}
		}

	    private int TotalClipsInQueue(AudioClip audioClip)
	    {
	        int value;
	        return _audioClips.TryGetValue(audioClip, out value) ? value : 0;
	    }

        private void AddClipToQueue(AudioClip audioClip)
        {
            int value;
            if (!_audioClips.TryGetValue(audioClip, out value))
                value = 0;
            _audioClips[audioClip] = value + 1;
        }

        private void RemoveClipFromQueue(AudioClip audioClip)
        {
            int value;
            if (!_audioClips.TryGetValue(audioClip, out value))
                return;

            if (value > 1)
                _audioClips[audioClip] = value - 1;
            else
                _audioClips.Remove(audioClip);
        }

        private float _timeFromLastLoadAttempt;
		private float _volume;
		private List<AudioSourceData> _audioSources = new List<AudioSourceData>();
		private Queue<AudioData> _objects = new Queue<AudioData>();
	    private List<AudioData> _loadingObjects = new List<AudioData>();

        private object _lockObject = new object();

		private class AudioData
		{
			public AudioClip AudioClip;
			public bool Loop;
			public int Id;
		    public int LoadAttempts;
		}

		private class AudioSourceData
		{
			public AudioSource audioSource;
			public int id;
		}

		//private SceneBeforeUnloadSignal _sceneBeforeUnloadSignal;
		private AppActivatedSignal _appActivatedSignal;
		private GameSettings _gameSettings;
	    private IResourceLocator _resourceLocator;
	    private Dictionary<AudioClip, int> _audioClips = new Dictionary<AudioClip, int>();
	}
}
