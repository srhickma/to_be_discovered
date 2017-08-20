using System;
using System.Collections;
using System.Collections.Generic;

public class SortedLinkedList<T> : IEnumerable<T> {
    
    private readonly LinkedList<T> list;
    private readonly Comparison<T> comparison;

    public SortedLinkedList(Comparison<T> comparison){
        list = new LinkedList<T>();
        this.comparison = comparison;
    }

    public void add(T element){
        LinkedListNode<T> currNode;
        for(currNode = list.First; currNode != null && comparison.Invoke(currNode.Value, element) < 0; currNode = currNode.Next){ }
        if(currNode == null){
            list.AddLast(element);
            return;
        }
        list.AddBefore(currNode, element);
    }

    public void add(params T[] elements){
        foreach(T element in elements){
            add(element);
        }
    }
    
    public void add(ICollection<T> elements){
        foreach(T element in elements){
            add(element);
        }
    }

    public T first(){
        return list.First.Value;
    }
    
    public T popFirst(){
        T first = this.first();
        list.RemoveFirst();
        return first;
    }

    public bool isEmpty(){
        return list.First == null;
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator(){
        return ((IEnumerable<T>) list).GetEnumerator();
    }
    
    IEnumerator IEnumerable.GetEnumerator(){
        return ((IEnumerable) list).GetEnumerator();
    }

}