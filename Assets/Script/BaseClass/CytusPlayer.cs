using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Playzone
{
    [SerializeField]
    private Vector2 center = Vector2.zero;
    public Vector2 Center => center;

    [SerializeField]
    private Vector2 size = new Vector2(0.8f, 0.9f);
    public Vector2 Size => size;
}

public abstract class CPlayer : MonoBehaviour
{
    public abstract float CurrentTime { get; }
    public abstract bool IsPlaying { get; }
    public abstract double Duration { get; }
}

public abstract class CorePlayerBase : CPlayer
{
    [Header("Playzone")]
    [SerializeField]
    private Rect playZone;
    public Rect PlayZone => playZone;
    [Header("UI")]
    [SerializeField]
    private ScoreDisplayer uiScoreText = null;
    [SerializeField]
    private ComboDisplayer uiComboText = null;

    public static CorePlayerBase ActivePlayer { get; set; }
    private int _score { get; set; }
    public int Score
    {
        get => _score;
        private set
        {
            _score = value;
            uiScoreText.Refresh(value);
        }
    }
    private int _combo { get; set; }
    public int Combo
    {
        get => _combo;
        private set
        {
            _combo = value;
            uiComboText.Refresh(value);
        }
    }

    private List<CoreFollower> followers = new List<CoreFollower>();
    public void RegistFollower(CoreFollower _follower)
    {
        if (!followers.Contains(_follower))
        {
            followers.Add(_follower);
            _follower.gameObject.SetActive(true);
        }
    }
    public void UnregistFollower(CoreFollower _follower)
    {
        if (!followers.Contains(_follower))
        {
            followers.Remove(_follower);
            _follower.gameObject.SetActive(false);
        }
    }
    private void UnleashFollowers()
    {
        followers.ForEach(x =>{ x.gameObject.SetActive(false); });
        followers.Clear();
    }

    public virtual void Play()
    {
        ActivePlayer = this;
        Score = 0;
        Combo = 0;

        if (IsPlaying != true)
            followers.ForEach(x => { x.OnPlay(); });
        else
            followers.ForEach(x => { x.OnResume(); });
    }
    public virtual void Pause()
    {
        followers.ForEach(x => { x.OnPause(); });
    }
    public virtual void Stop()
    {
        followers.ForEach(x => { x.OnStop(); });
        UnleashFollowers();
    }

    protected virtual void OnEnable()
    {
        NoteBase.aNoteScored += NoteBase_aNoteScored;
        NoteBase.aNoteMissed += NoteBase_aNoteMissed;
    }

    protected virtual void OnDisable()
    {
        NoteBase.aNoteScored -= NoteBase_aNoteScored;
        NoteBase.aNoteMissed -= NoteBase_aNoteMissed;
    }

    private void NoteBase_aNoteScored(NoteBase noteBase, HitType hitType)
    {
        switch (hitType)
        {
            case HitType.Perfect:
                Score += (300 + Combo);
                Combo++;
                uiComboText.Refresh(Combo);
                break;
            case HitType.Great:
                Score += (150 + Combo);
                uiComboText.Refresh(Combo);
                Combo++;
                break;
            case HitType.Good:
                Score += (75 + Combo);
                break;
        }
        uiScoreText.Refresh(Score);
    }

    private void NoteBase_aNoteMissed(NoteBase obj) => Combo = 0;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(playZone.center, playZone.size);
    }
}

public abstract class CoreFollower : CPlayer
{
    public CorePlayerBase CorePlayer => CorePlayerBase.ActivePlayer;

    public abstract void OnPlay();
    public abstract void OnPause();
    public abstract void OnResume();
    public abstract void OnStop();

    public sealed override float CurrentTime => CorePlayer.CurrentTime;
    public sealed override bool IsPlaying => CorePlayer.IsPlaying;
    public sealed override double Duration => CorePlayer.Duration;
}


