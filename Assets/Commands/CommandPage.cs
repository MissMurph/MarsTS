using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarsTS.Commands {

    [System.Serializable]
    public class CommandPage {

        [SerializeField]
        private string[] topRow = new string[3];

		[SerializeField]
		private string[] midRow = new string[3];

		[SerializeField]
		private string[] botRow = new string[3];

		//Hard-Coding should work 
        public string[,] Build () {
            string[,] output = new string[3,3];

			for (int i = 0; i < 3; i++) {
				output[0,i] = topRow[i];
			}

			for (int i = 0; i < 3; i++) {
				output[1, i] = midRow[i];
			}

			for (int i = 0; i < 3; i++) {
				output[2, i] = botRow[i];
			}

			return output;
		}
    }
}