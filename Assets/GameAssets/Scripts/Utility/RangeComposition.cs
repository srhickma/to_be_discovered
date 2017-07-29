﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
using UnityEngine;

public class RangeComposition {

    private readonly LinkedList<Range> ranges;
    
    public RangeComposition(){
        ranges = new LinkedList<Range>();
    }

    public RangeComposition(LinkedList<Range> ranges){
        if(ranges.First != null){
            for(LinkedListNode<Range> outerNode = ranges.First; outerNode.Next != null; outerNode = outerNode.Next){
                if(outerNode.Value.max > outerNode.Next.Value.min){
                    throw new RangeCompositionException("Range composition is not sorted");
                }
                for(LinkedListNode<Range> innerNode = outerNode.Next; innerNode != null; innerNode = innerNode.Next){
                    assertNoOverlap(outerNode.Value, innerNode.Value);
                }
            }
        }
        this.ranges = ranges;
    }

    public LinkedListNode<Range> addRange(Range range){
        LinkedListNode<Range> currNode = ranges.First;
        if(currNode == null){
            ranges.AddFirst(range);
            return ranges.First;
        }
        for(; currNode.Next != null && currNode.Value.max < range.min; currNode = currNode.Next){
            assertNoOverlap(currNode.Value, range);
        }
        if(currNode.Next == null && currNode.Value.max < range.min){
            ranges.AddAfter(currNode, range);
            return currNode.Next;
        }
        assertNoOverlap(currNode.Value, range);
        ranges.AddBefore(currNode, range);
        return currNode.Previous;
    }
    
    public void injectRange(Range range){
        LinkedListNode<Range> currNode = ranges.First;
        if(currNode == null){
            ranges.AddFirst(range);
            return;
        }
        if(range.min < currNode.Value.min){
            if(range.max < currNode.Value.min - 1){
                addRange(range);
                return;
            }
            Range newRange = new Range(range.min, currNode.Value.max);
            ranges.Remove(currNode);
            currNode = addRange(newRange);
            if(range.max <= newRange.max){
                return;
            }
        }
        while(currNode.Next != null && range.max > currNode.Value.max){
            if(range.min >= currNode.Next.Value.min){
                currNode = currNode.Next;
                continue;
            }
            Range newRange = range;
            if(range.max < currNode.Next.Value.min - 1){
                if(range.min <= currNode.Value.max + 1){
                    newRange = new Range(currNode.Value.min, range.max);
                    ranges.Remove(currNode);
                }
                addRange(newRange);
                return;
            }
            int newMax = currNode.Next.Value.max;
            newRange = new Range(range.min, newMax);
            ranges.Remove(currNode.Next);
            if(range.min <= currNode.Value.max + 1){
                newRange = new Range(currNode.Value.min, newMax);
                ranges.Remove(currNode);
            }
            currNode = addRange(newRange);
        }
        if(currNode.Next == null && currNode.Value.max < range.max){
            Range newRange = range;
            if(currNode.Value.max >= range.min){
                newRange = new Range(currNode.Value.min, range.max);
                ranges.Remove(currNode);
            }
            addRange(newRange);
        }
    }

    public void debug(){
        int size = compressedSize();
        string s = "";
        int[] array = new int[size];
        for(int i = 0; i < size; i ++){
            array[i] = expandedValueOf(i);
            s += array[i] + " ";
        }
        Debug.Log(s);
    }

    public RangeComposition inverseRangeComposition(Range universe){
        LinkedList<Range> newRanges = new LinkedList<Range>();
        int prevX = universe.min;
        LinkedListNode<Range> currNode = ranges.First;
        for(; currNode != null && currNode.Value.max < prevX; currNode = currNode.Next){}
        if(currNode != null && currNode.Value.contains(prevX)){
            prevX = currNode.Value.max;
        }
        for(; currNode != null && currNode.Value.min < universe.max; currNode = currNode.Next){
            int currX = currNode.Value.min;
            if(currX - prevX > 1){
                newRanges.AddLast(new Range(prevX + 1, currX - 1));
            }
            prevX = currNode.Value.max;
        }
        if(universe.max - prevX > 1){
            newRanges.AddLast(new Range(prevX + 1, universe.max - 1));
        }
        return new RangeComposition(newRanges);
    }

    public bool isEmpty(){
        return ranges.All(range => range.size() <= 0);
    }

    public int compressedSize(){
        return ranges.Sum(range => range.size());
    }

    public int expandedValueOf(int index){
        LinkedListNode<Range> currentNode = ranges.First;
        while(currentNode != null){
            int currentSize = currentNode.Value.size();
            if(index > currentSize - 1){
                index -= currentSize;
            }
            else{
                return currentNode.Value.min + index;
            }
            currentNode = currentNode.Next;
        }
        throw new RangeCompositionException("Index does not expand within range composition");
    }

    private static void assertNoOverlap(Range r1, Range r2){
        if(r1.overlapsWith(r2)){
            throw new RangeCompositionException("Overlapping ranges in composition");
        }
    }

    public LinkedList<Range> getRanges(){
        return ranges;
    }
    
    public class RangeCompositionException : Exception {
        public RangeCompositionException(string message) : base(message) { }
    }

}
