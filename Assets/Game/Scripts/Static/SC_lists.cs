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

    public List<SC_asteroid> SC_asteroid = new List<SC_asteroid>();
    public void AddTo_SC_asteroid(SC_asteroid ast) { SC_asteroid.Add(ast); }
    public void RemoveFrom_SC_asteroid(SC_asteroid ast) { SC_asteroid.Remove(ast); }

    public List<SC_boss> SC_boss = new List<SC_boss>();
    public void AddTo_SC_boss(SC_boss bos) { SC_boss.Add(bos); }
    public void RemoveFrom_SC_boss(SC_boss bos) { SC_boss.Remove(bos); }

    public List<SC_players> SC_players = new List<SC_players>();
    public void AddTo_SC_players(SC_players pla) { SC_players.Add(pla); }
    public void RemoveFrom_SC_players(SC_players pla) { SC_players.Remove(pla); }
}
