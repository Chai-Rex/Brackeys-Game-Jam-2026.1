using System;
using System.Collections.Generic;
using UnityEngine;
using UtilitySingletons;
public class TimeManager : Singleton<TimeManager> {

    [SerializeField] private float timeMultiplier = 1.0f;

    private class ScheduledEvent : IComparable<ScheduledEvent> {
        public float _TriggerTime;
        public Action _Callback;
        public bool _IsRepeating;
        public float _RepeatInterval;

        public int CompareTo(ScheduledEvent i_other) {
            return _TriggerTime.CompareTo(i_other._TriggerTime);
        }
    }

    private List<ScheduledEvent> _scheduledEvents = new List<ScheduledEvent>();
    private float _currentTime = 0f;
    private bool _isPaused = false;

    public float CurrentTime => _currentTime;
    public bool IsPaused => _isPaused;

    private void Update() {
        if (_isPaused) return;

        _currentTime += Time.deltaTime * timeMultiplier;

        // Process events that have reached their trigger time
        while (_scheduledEvents.Count > 0 && _scheduledEvents[0]._TriggerTime <= _currentTime) {
            ScheduledEvent evnt = _scheduledEvents[0];
            _scheduledEvents.RemoveAt(0);

            evnt._Callback?.Invoke();

            // If repeating, reschedule
            if (evnt._IsRepeating) {
                evnt._TriggerTime = _currentTime + evnt._RepeatInterval;
                InsertEvent(evnt);
            }
        }
    }

    /// <summary>
    /// Schedule a callback to be executed at a specific time
    /// </summary>
    public void ScheduleAt(float i_time, Action i_callback) {
        if (i_callback == null) {
            Debug.LogWarning("TimeManager: Cannot schedule null callback");
            return;
        }

        ScheduledEvent evnt = new ScheduledEvent {
            _TriggerTime = i_time,
            _Callback = i_callback,
            _IsRepeating = false
        };

        InsertEvent(evnt);
    }

    /// <summary>
    /// Schedule a callback to be executed after a delay from current time
    /// </summary>
    public void ScheduleAfter(float i_delay, Action i_callback) {
        ScheduleAt(_currentTime + i_delay, i_callback);
    }

    /// <summary>
    /// Schedule a repeating callback starting at a specific time
    /// </summary>
    public void ScheduleRepeatingAt(float i_startTime, float i_interval, Action i_callback) {
        if (i_callback == null) {
            Debug.LogWarning("TimeManager: Cannot schedule null callback");
            return;
        }

        ScheduledEvent evnt = new ScheduledEvent {
            _TriggerTime = i_startTime,
            _Callback = i_callback,
            _IsRepeating = true,
            _RepeatInterval = i_interval
        };

        InsertEvent(evnt);
    }

    /// <summary>
    /// Schedule a repeating callback starting after a delay from current time
    /// </summary>
    public void ScheduleRepeatingAfter(float i_delay, float i_interval, Action i_callback) {
        ScheduleRepeatingAt(_currentTime + i_delay, i_interval, i_callback);
    }

    /// <summary>
    /// Cancel all scheduled events for a specific callback
    /// </summary>
    public void CancelScheduled(Action i_callback) {
        _scheduledEvents.RemoveAll(evnt => evnt._Callback == i_callback);
    }

    /// <summary>
    /// Cancel all scheduled events
    /// </summary>
    public void CancelAllScheduled() {
        _scheduledEvents.Clear();
    }

    /// <summary>
    /// Pause the time manager
    /// </summary>
    public void Pause() {
        _isPaused = true;
    }

    /// <summary>
    /// Resume the time manager
    /// </summary>
    public void Resume() {
        _isPaused = false;
    }

    /// <summary>
    /// Toggle pause state
    /// </summary>
    public void TogglePause() {
        _isPaused = !_isPaused;
    }

    /// <summary>
    /// Reset the current time to zero and clear all scheduled events
    /// </summary>
    public void Reset() {
        _currentTime = 0f;
        _scheduledEvents.Clear();
    }

    /// <summary>
    /// Get current in-game time with conversion rate
    /// </summary>
    public string GetTime() {
        return _currentTime.ToString();
    }

    private void InsertEvent(ScheduledEvent i_event) {
        // Binary search to find insertion point to keep list sorted
        int index = _scheduledEvents.BinarySearch(i_event);
        if (index < 0)
            index = ~index;

        _scheduledEvents.Insert(index, i_event);
    }


}
