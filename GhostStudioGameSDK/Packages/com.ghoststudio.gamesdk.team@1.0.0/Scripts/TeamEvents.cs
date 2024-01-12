using API.V1.Game;
using AppBase.Event;

public struct EventTeamBasicChanged : IEvent
{
    public ClubBasic newBasic;
}

public struct EventTeamMemberUpdate : IEvent
{
    
}

public struct EventOnJoinTeam : IEvent
{
        
}

public struct EventOnConfirmSelfTeamInfo : IEvent
{
        
}
