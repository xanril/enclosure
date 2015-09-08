﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using JsonFx.Json;

public class WorksheetJsonConverter : JsonConverter {

	public override bool CanConvert (Type t) {
		
		return t == typeof(WorksheetQuery);
	}

	public override Dictionary<string, object> WriteJson (Type type, object value) {
		
		return null;
	}

	public override object ReadJson (Type type, Dictionary<string, object> value) {
		
		Dictionary<string, object> genObject; // generic object used to read json info
		//Dictionary<string, object>[] genObjArray; 
		WorksheetQuery wkQuery = new WorksheetQuery();
		
		try {
			
			Dictionary<string, object> feedDict = value["feed"] as Dictionary<string, object>;
			genObject = feedDict["title"] as Dictionary<string, object>;
			wkQuery.title = genObject["$t"] as String;
			
			Dictionary<string,object>[] entryArray = feedDict["entry"] as Dictionary<string, object>[];
			
			int maxEntries = entryArray.Length;
			for(int i = 0; i < maxEntries; i++) {
				
				// parse row number and row column from ID url.
				// get from last part of the URL string.
				// sample: https://spreadsheets.google.com/feeds/cells/1viaHzqpQBe8DIz9OAsdTHyHHl0tjxoaxOZ9UySiLesg/od6/public/basic/R1C1
				// we need to extract "R1C1"
				genObject = entryArray[i] as Dictionary<string, object>;
				genObject = genObject["id"] as Dictionary<string, object>;
				int[] cellIndex = ParseCellIndex(genObject["$t"] as string);

				genObject = entryArray[i] as Dictionary<string, object>;
				genObject = genObject["content"] as Dictionary<string, object>;
				string cellContents = genObject["$t"] as string;

				wkQuery.AddCellContent(cellIndex[0], cellIndex[1], cellContents);
			}
		}
		catch(Exception ex) {
			
			Debug.LogError("WorksheetJsonConverter: Error deserializing json. \nError: " + ex.Message);
			return null; 
		}
		
		return wkQuery;
	}
	
	private int[] ParseCellIndex(string cellID) {
		
		int lastIdxSlash = cellID.LastIndexOf('/') + 1;
		string substringRowCol = cellID.Substring(lastIdxSlash, cellID.Length - lastIdxSlash);

		string row = substringRowCol.Substring(1, substringRowCol.IndexOf('C') - 1);
		string col = substringRowCol.Substring(substringRowCol.IndexOf('C') + 1);

		int[] cellIndex = new int[2];
		int parseResult = 0;

		if(Int32.TryParse(row, out parseResult)) {
			cellIndex[0] = parseResult;
		}

		if(Int32.TryParse(col, out parseResult)) {
			cellIndex[1] = parseResult;
		}

		return cellIndex;
	}
}
