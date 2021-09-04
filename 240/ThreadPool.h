#ifndef _THREAD_POOL_H_
#define _THREAD_POOL_H_

#include <stack>
#include <thread>
#include <mutex>
#include <condition_variable>
#include <algorithm>
#include <vector>
#include <functional>

class ThreadPool
{
private:
    struct Worker
    {
        ThreadPool& pool_;
        Worker(ThreadPool& pool);

        void operator () ();
    };
public:
    ThreadPool(int thread_count);
    virtual ~ThreadPool();
    /*!
        \param t thread functor thread pool will execute<br>
            eg) std::bind(&some_function, arg1, arg2)
        */
    template <class T>
    void PostTask(T t)
    {
        std::unique_lock<std::mutex> lock(mutex_);
        tasks_.push_back(t);
        condition_.notify_one();
    }

    void Join();
    size_t GetTaskCount();
private:
    std::vector<std::thread > workers_;
    std::deque<std::function<void()> > tasks_;
    bool stop_;
    std::mutex mutex_;
    std::condition_variable condition_;
};

#endif