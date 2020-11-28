public static class FormationGeneratorExtensions
{
    // return all formations belong to this clan
    public static Formation[] GetFormations(this FormationGenerator manager, FormationClan clan)
    {
        InfluenceMap influence = clan == FormationClan.Enemy ?
            manager.enemyInfluence : manager.playerInfluence;

        int n = influence.friends.Count;
        Formation[] formations = new Formation[n];

        int i = 0;
        foreach (Formation form in influence.friends)
        {
            formations[i] = form;
            i++;
        }
        return formations;
    }
}
