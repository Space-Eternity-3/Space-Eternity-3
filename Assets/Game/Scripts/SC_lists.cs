using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_lists : MonoBehaviour
{
    public List<SC_bullet> SC_bullet = new List<SC_bullet>();
    public void AddTo_SC_bullet(SC_bullet bul) { SC_bullet.Add(bul); }
    public void RemoveFrom_SC_bullet(SC_bullet bul) { SC_bullet.Remove(bul); }

    public List<SC_seeking> SC_seeking = new List<SC_seeking>();
    public void AddTo_SC_seeking(SC_seeking sek) { SC_seeking.Add(sek); }
    public void RemoveFrom_SC_seeking(SC_seeking sek) { SC_seeking.Remove(sek); }

    public List<SC_pulse_bar> SC_pulse_bar = new List<SC_pulse_bar>();
    public void AddTo_SC_pulse_bar(SC_pulse_bar pba) { SC_pulse_bar.Add(pba); }
    public void RemoveFrom_SC_pulse_bar(SC_pulse_bar pba) { SC_pulse_bar.Remove(pba); }

    public List<SC_resp_blocker> SC_resp_blocker = new List<SC_resp_blocker>();
    public void AddTo_SC_resp_blocker(SC_resp_blocker rpb) { SC_resp_blocker.Add(rpb); }
    public void RemoveFrom_SC_resp_blocker(SC_resp_blocker rpb) { SC_resp_blocker.Remove(rpb); }

    public List<SC_asteroid> SC_asteroid = new List<SC_asteroid>();
    public void AddTo_SC_asteroid(SC_asteroid ast) { SC_asteroid.Add(ast); }
    public void RemoveFrom_SC_asteroid(SC_asteroid ast) { SC_asteroid.Remove(ast); }

    public List<SC_fobs> SC_fobs = new List<SC_fobs>();
    public void AddTo_SC_fobs(SC_fobs fob) { SC_fobs.Add(fob); }
    public void RemoveFrom_SC_fobs(SC_fobs fob) { SC_fobs.Remove(fob); }

    public List<SC_boss> SC_boss = new List<SC_boss>();
    public void AddTo_SC_boss(SC_boss bos) { SC_boss.Add(bos); }
    public void RemoveFrom_SC_boss(SC_boss bos) { SC_boss.Remove(bos); }

    public List<SC_point_expand> SC_point_expand = new List<SC_point_expand>();
    public void AddTo_SC_point_expand(SC_point_expand pex) { SC_point_expand.Add(pex); }
    public void RemoveFrom_SC_point_expand(SC_point_expand pex) { SC_point_expand.Remove(pex); }
}
