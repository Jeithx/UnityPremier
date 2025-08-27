using UnityEngine;

public class VideoController : MonoBehaviour
{
    private VideoContent vc;
    private Timer timer;
    private TimelineGrid timeline;

    [SerializeField] private float epsilon = 0.05f;
    private bool started;
    private bool finished;

    public static VideoController Create(
        GameObject host,
        Timer time,
        VideoContent videoContent,
        TimelineGrid tlg)
    {
        var ctrl = host.AddComponent<VideoController>();
        ctrl.Init(time, videoContent, tlg);
        return ctrl;
    }

    public void Init(Timer time, VideoContent videoContent, TimelineGrid tlg)
    {
        timer = time;
        vc = videoContent;
        timeline = tlg;
        started = false;
        finished = false;
    }

    public VideoContent getvc() => vc;

    private void Update()
    {
        if (!timer || !timeline) return;
        if (!timeline.getFlag()) return;

        float t = timer.getCurrentTime();
        float start = vc.getStart();
        float end = vc.getEnd();

        if (!started && t >= start - epsilon && t <= end + epsilon)
        {
            started = true;
            finished = false;
            timeline.ShowContent(vc, vc.GetTexture());  // layered window
            vc.playVideo();
        }

        if (started && !finished && t >= end - epsilon)
        {
            finished = true;
            vc.stopVideo();
            timeline.HideContent(vc);
        }
    }

    public void ScrubTo(float globalTime, bool shouldPlay)
    {
        if (vc == null) return;

        float start = vc.getStart();
        float end = vc.getEnd();

        if (globalTime < start - epsilon || globalTime > end + epsilon)
        {
            vc.stopVideo();
            started = false;
            finished = false;
            timeline.HideContent(vc);
            return;
        }

        timeline.ShowContent(vc, vc.GetTexture());

        float local = Mathf.Clamp(globalTime - start, 0f, vc.getLength());
        vc.SeekToSeconds(local, shouldPlay);

        started = shouldPlay || local > 0f;
        finished = false;
    }

    public void ForceStop()
    {
        if (vc != null) vc.stopVideo();
        if (timeline != null) timeline.HideContent(vc);
    }

    public void ResetState()
    {
        started = false;
        finished = false;
    }
}
