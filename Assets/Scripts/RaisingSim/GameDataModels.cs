using System;

namespace RaisingSim
{
    [Serializable]
    public class InitialStatsDatabase
    {
        public StatValue[] stats;
    }

    [Serializable]
    public class ActionDatabase
    {
        public ActionData[] actions;
    }

    [Serializable]
    public class EventDatabase
    {
        public EventData[] events;
    }

    [Serializable]
    public class FestivalDatabase
    {
        public FestivalData[] festivals;
    }

    [Serializable]
    public class FestivalRewardDatabase
    {
        public FestivalRewardData[] rewards;
    }

    [Serializable]
    public class EndingDatabase
    {
        public EndingData[] endings;
    }

    [Serializable]
    public class StatValue
    {
        public string stat;
        public int initial_value;
        public bool has_min;
        public int min;
        public bool has_max;
        public int max;
        public string description;
    }

    [Serializable]
    public class StatDelta
    {
        public string stat;
        public float amount;
    }

    [Serializable]
    public class ActionData
    {
        public string action_id;
        public string name;
        public string category;
        public string time;
        public StatDelta[] deltas;
        public string result_text;
        public string unlock_condition;
        public string memo;
    }

    [Serializable]
    public class EventData
    {
        public string event_id;
        public string name;
        public string trigger;
        public int priority;
        public bool once;
        public string condition;
        public string dialogue_text;
        public string default_delta;
        public EventChoiceData[] choices;
    }

    [Serializable]
    public class EventChoiceData
    {
        public string text;
        public string condition;
        public string delta;
        public string next_event_id;
    }

    [Serializable]
    public class FestivalData
    {
        public string festival_id;
        public int year;
        public string season;
        public string name;
        public bool consumes_action_slot;
        public string score_formula;
        public string intro_event_id;
        public string result_group;
        public string memo;
        public string unity_note;
    }

    [Serializable]
    public class FestivalRewardData
    {
        public string result_group;
        public int min_score;
        public int max_score;
        public string result_name;
        public string delta;
        public string item_id;
        public string result_event_id;
        public string memo;
    }

    [Serializable]
    public class EndingData
    {
        public string ending_id;
        public int priority;
        public string name;
        public string condition;
        public string ending_text;
        public string memo;
    }
}
