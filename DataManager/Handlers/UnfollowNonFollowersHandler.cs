﻿namespace DataManager.Handlers;
public class UnfollowNonFollowersHandler : BaseOperationHandler
{
    public override bool RequiresFile => false;

    protected override void Execute(Dictionary<string, object> parameters)
    {
        throw new NotImplementedException();
    }
}