using UnityEngine;
using System.Collections;

public interface IProcedure
{
	void StartTest(float[] param, float maxErr=100000f);

	bool IsDone();

	float GetErr();
}

public class Twiddle : MonoBehaviour {

	public float thresh = 0.0001f;

	public float[] param_list;
	public float[] dp;
	public IProcedure proc;
	public float best_err = 10000f;
	public float waitSampleTest = 0.5f;

	//if zero or higher, use this to focus on a single param.
	public int focusOnSinglaParam = -1;

	// Use this for initialization
	public void StartTest(IProcedure iProc, float[] initial_params) 
	{
		proc = iProc;

		//copy params as the initial params
		param_list = new float[initial_params.Length];
		dp = new float[initial_params.Length];

		for(int iP = 0; iP < initial_params.Length; iP++)
		{
			param_list[iP] = initial_params[iP];
			//dp[iP] = 1.0f;
			dp[iP] = 0.5f * initial_params[iP];
		}

		//Kick off coroutine so our procedure
		//can evaluate asyncronously.
		StartCoroutine(EvalRun());
	}

	float SumOfDp()
	{
		float sum_dp = 0.0f;

		foreach(float val in dp)
			sum_dp += val;

		return sum_dp;
	}

	IEnumerator EvalRun()
	{
		float err = 0f;
		int iTest = 1;

		proc.StartTest(param_list);

		while(!proc.IsDone())
			yield return new WaitForSeconds(waitSampleTest);

		best_err = proc.GetErr();

		while (SumOfDp() > thresh)
		{
			for(int iParam = 0; iParam < dp.Length; iParam++)
			{
				if(focusOnSinglaParam != -1 && iParam != focusOnSinglaParam)
					continue;

				param_list[iParam] += dp[iParam];

				Debug.Log(string.Format("Test {0} iParam {1}", iTest++, iParam));
				Debug.Log(string.Format("Param {0} dp {1}", param_list[iParam], dp[iParam]));

				proc.StartTest(param_list, best_err);
				
				while(!proc.IsDone())
					yield return new WaitForSeconds(waitSampleTest);
				
				err = proc.GetErr();

				if(err < best_err)
				{
					best_err = err;

					//try a larger change next time
					dp[iParam] *= 1.1f;
				}
				else
				{
					//try the same change but in the other direction.
					param_list[iParam] -= 2 * dp[iParam];

					Debug.Log(string.Format("Test {0} iParam {1}", iTest++, iParam));
					Debug.Log(string.Format("Param {0} dp {1}", param_list[iParam], dp[iParam]));

					proc.StartTest(param_list, best_err);
					
					while(!proc.IsDone())
						yield return new WaitForSeconds(waitSampleTest);
					
					err = proc.GetErr();

					if(err < best_err)
					{
						best_err = err;

						//error reduced, so try larger change in this dir.
						dp[iParam] *= 1.1f;
					}
					else
					{
						//restore param to initial value
						param_list[iParam] += dp[iParam];

						//try smaller change.
						dp[iParam] *= 0.9f;
					}
				}
			}
		}
	}
}
