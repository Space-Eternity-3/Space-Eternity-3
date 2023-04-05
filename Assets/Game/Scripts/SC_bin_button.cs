using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_bin_button : MonoBehaviour
{
    public Button button;
    public SC_slots SC_slots;
    public Text text;

    public int actual_funny_text = 0;
    string[] funny_texts = new string[]
{
//#1
"Behold, the endless abyss of cosmic trash.",
"Congratulations, you've made the galaxy a landfill.",
"From Earth to the edges of infinity, your garbage reigns supreme.",
"Space debris: the gift that keeps on giving.",
"The cosmos is now littered with your discarded belongings.",
"The infinite cosmos now bears your discarded items.",
"The stars now shine upon your waste.",
"Your debris has found a new home among the stars.",
"Your discarded items are now cosmic relics.",
"Your garbage has now become a part of the cosmic ecosystem.",
"Your garbage has now become an inseparable part of the universe.",
"Your garbage is floating in the void, forever immortalized.",
"Your garbage is now a permanent fixture in the cosmos.",
"Your junk has now joined the celestial clutter.",
"Your junk is now interstellar.",
"Your litter has become a permanent fixture in the cosmos.",
"Your litter is now a part of the universe's legacy.",
"Your refuse has taken its place among the stars.",
"Your refuse has now become a part of the universe's history.",
"Your trash has embarked on an intergalactic adventure.",
"Your trash has traveled farther than any human ever will.",
"Your trash heap now spans the universe.",
"Your waste has gone where no trash has gone before.",
"Your waste has now become cosmic debris.",
"Your waste has now become a part of the universe's history.",
"Your waste has now become a part of the cosmic ecosystem.",
"Your waste is now immortalized in the stars.",
"You've added a new dimension to the universe: your garbage.",
"You've contributed to the infinite garbage dump in the sky.",
"You've created a celestial junkyard.",
"You've given the universe a gift it didn't want: your garbage.",
"You've left a trail of trash across the universe.",
"You've left your mark on the universe with your garbage.",
"You've made the universe a little dirtier with your trash.",
"You've made your mark on the universe with your trash.",
"You've unleashed your trash upon the cosmos.",
"Space: the final frontier for your unwanted possessions.",
"The universe now bears witness to your refuse.",
"The universe will never forget the impact of your trash.",
"The vast expanse of space now bears your refuse.",
"The universe now carries the burden of your garbage.",
"The universe now carries the weight of your discarded belongings.",
"The universe now bears the weight of your discarded belongings.",
"The universe now bears the weight of your garbage.",
"You've turned the universe into your personal trash can.",
"Your garbage is now a permanent resident of the universe.",
"Your litter now roams the cosmos.",
"Your trash now pollutes the cosmic landscape.",
"Your waste has taken over the universe.",
"Your discarded items now float aimlessly through the cosmos.",
//#2
"Your trash has left a lasting impression on the cosmos.",
"Your garbage has found a new home among the stars, and it's not leaving anytime soon.",
"Your waste has added a new layer of complexity to the universe.",
"You've given the universe a new challenge to overcome: your waste.",
"The universe now hosts a galactic landfill, thanks to you.",
"Your trash has become a part of the universe's fabric.",
"Your garbage has become a part of the universe's DNA.",
"Your discarded items are now cosmic orphans, lost in the vastness of space.",
"The cosmos is now a little bit dirtier, thanks to your litter.",
"Your waste has become the universe's problem.",
"Your trash is now a permanent fixture in the celestial landscape.",
"Your garbage has now become a cosmic legend.",
"Your discarded items have achieved interstellar fame.",
"Your litter now floats through the cosmos, a testament to human waste.",
"Your waste has taken on a life of its own, roaming the universe without end.",
"Your trash has transcended the boundaries of Earth and entered the infinite expanse of space.",
"Your garbage has joined the ranks of the universe's most notorious villains.",
"The universe now carries the burden of your waste, forever.",
"Your litter has become an eternal reminder of human excess.",
"Your trash is now a permanent reminder of our impact on the universe.",
"Your discarded items have become a part of the cosmic history book.",
"Your waste has become a permanent part of the universe's narrative.",
"Your garbage is now a universal symbol of human negligence.",
"The cosmos now bears the scars of your waste.",
"Your trash has become a cosmic anomaly.",
"Your litter is now a part of the universe's story.",
"Your waste has become a part of the universe's mythos.",
"Your garbage has become a part of the cosmic legacy.",
"The universe now knows your name, and it's not a good thing.",
"Your trash has become a cosmic mystery, floating through space without purpose.",
"Your discarded items have become a part of the cosmic tapestry.",
"Your waste has become a part of the cosmic orchestra, adding a new note to the song of the universe.",
"Your garbage is now a part of the cosmic ballet.",
"Your litter has become a cosmic tragedy.",
"Your waste has become a cosmic comedy.",
"Your trash has become a cosmic horror story.",
"The universe now bears the brunt of your negligence.",
"Your garbage has become a cosmic anomaly, a puzzle for future generations to solve.",
"Your litter has become a cosmic riddle, a mystery waiting to be unraveled.",
"Your waste has become a cosmic conundrum, a puzzle with no clear solution.",
"Your trash has become a cosmic enigma, a question mark in the vastness of space.",
"Your garbage has become a cosmic paradox, a contradiction that defies explanation.",
"Your discarded items have become a cosmic puzzle, a mystery waiting to be solved.",
"Your waste has become a cosmic marvel, a wonder of the universe.",
"Your litter has become a cosmic spectacle, a sight to behold in the depths of space.",
"Your trash has become a cosmic phenomenon, a spectacle that defies explanation.",
"Your garbage has become a cosmic oddity, a quirk of the universe.",
"The universe now carries the weight of your neglect, a burden that will never be lifted.",
"Your waste has become a cosmic tragedy, a story of human excess and neglect.",
"Your litter has become a cosmic epic, a tale of human folly and hubris.",
"Your garbage has become a cosmic legend, a story that will be told for eons to come.",
"Your discarded items have become a cosmic saga, a story with no end in sight.",
//#3
"This game had no garbage collector for such a long time.",
"It might have been a bit radioacive. Who cares!",
"You've broken a gift from Kamiloso."
};

    void Update()
    {
        button.interactable =  (SC_slots.BackpackY[16]>0);
    }
    public void UpdateText()
    {
        int old_text = actual_funny_text;
        do{ actual_funny_text = UnityEngine.Random.Range(0,funny_texts.Length); }while(actual_funny_text==old_text);
        text.text = funny_texts[actual_funny_text];
    }
}
